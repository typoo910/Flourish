using ArkheideSystem.Flourish.Abstract;
using ArkheideSystem.Flourish.Services;

namespace ArkheideSystem.Flourish.Test.Services;

public sealed class CommandParserTests
{
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Parse_WithMissingCommandKey_ReturnsFalseWithoutCallingParsers(string? commandKey)
    {
        var parser = new StubCommandParser(_ => true);
        var sut = new CommandParser([parser]);

        var handled = sut.Parse(commandKey);

        Assert.False(handled);
        Assert.Empty(parser.ReceivedKeys);
    }

    [Fact]
    public void Parse_WhenFirstParserHandlesCommand_StopsAtFirstHandler()
    {
        var firstParser = new StubCommandParser(_ => true);
        var secondParser = new StubCommandParser(_ => throw new InvalidOperationException());
        var sut = new CommandParser([firstParser, secondParser]);

        var handled = sut.Parse("gallery.open");

        Assert.True(handled);
        Assert.Equal(["gallery.open"], firstParser.ReceivedKeys);
        Assert.Empty(secondParser.ReceivedKeys);
    }

    [Fact]
    public void Parse_WhenNoParserHandlesCommand_CallsEveryParserInOrder()
    {
        var calls = new List<string>();
        var firstParser = new StubCommandParser(key =>
        {
            calls.Add($"first:{key}");
            return false;
        });
        var secondParser = new StubCommandParser(key =>
        {
            calls.Add($"second:{key}");
            return false;
        });
        var sut = new CommandParser([firstParser, secondParser]);

        var handled = sut.Parse("gallery.unknown");

        Assert.False(handled);
        Assert.Equal(
            ["first:gallery.unknown", "second:gallery.unknown"],
            calls
        );
    }

    [Fact]
    public void Constructor_MaterializesParserCollection()
    {
        var initialParser = new StubCommandParser(_ => false);
        var laterParser = new StubCommandParser(_ => true);
        var parsers = new List<ICommandParser> { initialParser };
        var sut = new CommandParser(parsers);
        parsers.Add(laterParser);

        var handled = sut.Parse("gallery.open");

        Assert.False(handled);
        Assert.Equal(["gallery.open"], initialParser.ReceivedKeys);
        Assert.Empty(laterParser.ReceivedKeys);
    }

    [Fact]
    public async Task ExecuteAsync_RuntimeHandlerReceivesInvocationContextAndValue()
    {
        var sut = new CommandParser([]);
        CommandContext? receivedContext = null;
        CancellationToken receivedToken = default;
        var parameter = new object();
        sut.Register(
            "editor.save",
            (context, cancellationToken) =>
            {
                receivedContext = context;
                receivedToken = cancellationToken;
                return ValueTask.FromResult(CommandResult.HandledWith("saved"));
            }
        );
        using var cancellationSource = new CancellationTokenSource();

        var result = await sut.ExecuteAsync(
            "editor.save",
            parameter,
            CommandSource.Toolbar,
            cancellationSource.Token
        );

        Assert.Equal(CommandExecutionStatus.Handled, result.Status);
        Assert.Equal("saved", result.Value);
        Assert.NotNull(receivedContext);
        Assert.Equal("editor.save", receivedContext.CommandKey);
        Assert.Same(parameter, receivedContext.Parameter);
        Assert.Equal(CommandSource.Toolbar, receivedContext.Source);
        Assert.Equal(cancellationSource.Token, receivedToken);
    }

    [Fact]
    public async Task ExecuteAsync_WhenPredicateIsFalse_ReturnsDisabledWithoutInvokingHandler()
    {
        var invoked = false;
        var sut = new CommandParser([]);
        sut.Register(
            "editor.save",
            (_, _) =>
            {
                invoked = true;
                return ValueTask.FromResult(CommandResult.Handled);
            },
            _ => false
        );

        var result = await sut.ExecuteAsync("editor.save");

        Assert.Equal(CommandExecutionStatus.Disabled, result.Status);
        Assert.False(result.IsHandled);
        Assert.False(invoked);
        Assert.False(sut.CanExecute("editor.save"));
    }

    [Fact]
    public async Task ExecuteAsync_RuntimeNotHandled_FallsBackToLegacyParsers()
    {
        var legacy = new StubCommandParser(key => key == "reports.export");
        var sut = new CommandParser([legacy]);
        sut.Register(
            "reports.export",
            (_, _) => ValueTask.FromResult(CommandResult.NotHandled)
        );

        var result = await sut.ExecuteAsync("reports.export");

        Assert.True(result.IsHandled);
        Assert.Equal(["reports.export"], legacy.ReceivedKeys);
    }

    [Fact]
    public async Task ExecuteAsync_HandlerException_IsCapturedWithoutInvokingFallback()
    {
        var legacy = new StubCommandParser(_ => true);
        var sut = new CommandParser([legacy]);
        var failure = new InvalidOperationException("save failed");
        sut.Register(
            "editor.save",
            (_, _) => throw failure
        );

        var result = await sut.ExecuteAsync("editor.save");

        Assert.Equal(CommandExecutionStatus.Failed, result.Status);
        Assert.Same(failure, result.Exception);
        Assert.Empty(legacy.ReceivedKeys);
    }

    [Fact]
    public async Task ExecuteAsync_LegacyParserException_IsCaptured()
    {
        var failure = new InvalidOperationException("legacy failed");
        var sut = new CommandParser([new StubCommandParser(_ => throw failure)]);

        var result = await sut.ExecuteAsync("legacy.command");

        Assert.Equal(CommandExecutionStatus.Failed, result.Status);
        Assert.Same(failure, result.Exception);
    }

    [Fact]
    public async Task ExecuteAsync_WithCanceledToken_ReturnsCanceledWithoutCallingHandlers()
    {
        var invoked = false;
        var sut = new CommandParser([]);
        sut.Register(
            "editor.save",
            (_, _) =>
            {
                invoked = true;
                return ValueTask.FromResult(CommandResult.Handled);
            }
        );
        using var cancellationSource = new CancellationTokenSource();
        cancellationSource.Cancel();

        var result = await sut.ExecuteAsync(
            "editor.save",
            cancellationToken: cancellationSource.Token
        );

        Assert.Equal(CommandExecutionStatus.Canceled, result.Status);
        Assert.False(invoked);
    }

    [Fact]
    public void Register_WithDuplicateKeyAndRejectPolicy_Throws()
    {
        var sut = new CommandParser([]);
        sut.Register(
            "editor.save",
            (_, _) => ValueTask.FromResult(CommandResult.Handled)
        );

        var exception = Assert.Throws<InvalidOperationException>(() =>
            sut.Register(
                "editor.save",
                (_, _) => ValueTask.FromResult(CommandResult.Handled)
            )
        );

        Assert.Contains("editor.save", exception.Message);
        Assert.Single(sut.Registrations);
    }

    [Fact]
    public async Task Register_WithReplacePolicy_InvalidatesOldLeaseAndUsesReplacement()
    {
        var calls = new List<string>();
        var sut = new CommandParser([]);
        var original = sut.Register(
            "editor.save",
            (_, _) =>
            {
                calls.Add("original");
                return ValueTask.FromResult(CommandResult.Handled);
            }
        );

        var replacement = sut.Register(
            "editor.save",
            (_, _) =>
            {
                calls.Add("replacement");
                return ValueTask.FromResult(CommandResult.Handled);
            },
            options: new CommandRegistrationOptions
            {
                DuplicatePolicy = CommandDuplicatePolicy.Replace,
            }
        );

        Assert.False(original.IsRegistered);
        Assert.True(replacement.IsRegistered);
        original.Dispose();
        var result = await sut.ExecuteAsync("editor.save");
        Assert.True(result.IsHandled);
        Assert.Equal(["replacement"], calls);
    }

    [Fact]
    public async Task Register_WithAppendPolicy_UsesPriorityThenRegistrationOrder()
    {
        var calls = new List<string>();
        var sut = new CommandParser([]);
        sut.Register(
            "editor.save",
            (_, _) =>
            {
                calls.Add("low");
                return ValueTask.FromResult(CommandResult.Handled);
            },
            options: new CommandRegistrationOptions { Priority = -1 }
        );
        sut.Register(
            "editor.save",
            (_, _) =>
            {
                calls.Add("high");
                return ValueTask.FromResult(CommandResult.Handled);
            },
            options: new CommandRegistrationOptions
            {
                DuplicatePolicy = CommandDuplicatePolicy.Append,
                Priority = 10,
            }
        );

        var result = await sut.ExecuteAsync("editor.save");

        Assert.True(result.IsHandled);
        Assert.Equal(["high"], calls);
    }

    [Fact]
    public async Task Dispose_UnregistersOnlyOwnedHandlerAndRaisesChangedSnapshot()
    {
        var sut = new CommandParser([]);
        var changes = new List<CommandRegistryChangedEventArgs>();
        sut.Changed += (_, args) => changes.Add(args);
        var first = sut.Register(
            "editor.save",
            (_, _) => ValueTask.FromResult(CommandResult.NotHandled)
        );
        var second = sut.Register(
            "editor.save",
            (_, _) => ValueTask.FromResult(CommandResult.Handled),
            options: new CommandRegistrationOptions
            {
                DuplicatePolicy = CommandDuplicatePolicy.Append,
            }
        );

        first.Dispose();
        first.Dispose();

        Assert.False(first.IsRegistered);
        Assert.True(second.IsRegistered);
        Assert.Single(sut.Registrations);
        Assert.Equal(3, changes.Count);
        Assert.Equal(CommandRegistryChangeKind.Unregistered, changes[^1].ChangeKind);
        Assert.Single(changes[^1].Registrations);
        Assert.True((await sut.ExecuteAsync("editor.save")).IsHandled);
    }

    [Fact]
    public void NotifyCanExecuteChanged_FromLeaseAndRegistry_IdentifiesAffectedCommands()
    {
        var sut = new CommandParser([]);
        var affectedKeys = new List<string?>();
        sut.CanExecuteChanged += (_, args) => affectedKeys.Add(args.CommandKey);
        var registration = sut.Register(
            "editor.save",
            (_, _) => ValueTask.FromResult(CommandResult.Handled)
        );

        registration.NotifyCanExecuteChanged();
        sut.NotifyCanExecuteChanged();
        registration.Dispose();
        registration.NotifyCanExecuteChanged();

        Assert.Equal(["editor.save", null], affectedKeys);
    }

    [Fact]
    public async Task Parse_WithAsynchronousRuntimeHandler_DoesNotBlockLegacyShellEntryPoint()
    {
        var entered = new TaskCompletionSource(
            TaskCreationOptions.RunContinuationsAsynchronously
        );
        var release = new TaskCompletionSource(
            TaskCreationOptions.RunContinuationsAsynchronously
        );
        var sut = new CommandParser([]);
        sut.Register(
            "editor.save",
            async (_, _) =>
            {
                entered.TrySetResult();
                await release.Task;
                return CommandResult.Handled;
            }
        );

        var accepted = sut.Parse("editor.save");

        Assert.True(accepted);
        await entered.Task.WaitAsync(TimeSpan.FromSeconds(2));
        release.TrySetResult();
    }

    private sealed class StubCommandParser(Func<string, bool> parse) : ICommandParser
    {
        public List<string> ReceivedKeys { get; } = [];

        public bool TryParse(string commandKey)
        {
            ReceivedKeys.Add(commandKey);
            return parse(commandKey);
        }
    }
}
