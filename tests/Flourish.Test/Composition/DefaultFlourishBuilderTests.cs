using System.Windows.Controls;
using ArkheideSystem.Flourish.Abstract;
using ArkheideSystem.Flourish.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ArkheideSystem.Flourish.Test.Composition;

public sealed class DefaultFlourishBuilderTests
{
    [Fact]
    public void ConfigureMethods_WithNullCallbacks_ThrowArgumentNullExceptionImmediately()
    {
        var builder = FlourishBuilder.CreateDefaultBuilder([]);

        Assert.Throws<ArgumentNullException>(() => builder.ConfigureData(null!));
        Assert.Throws<ArgumentNullException>(() => builder.ConfigureServices(null!));
        Assert.Throws<ArgumentNullException>(() => builder.ConfigureShell(null!));
        Assert.Throws<ArgumentNullException>(() => builder.ConfigureTitleBar(null!));
        Assert.Throws<ArgumentNullException>(() => builder.ConfigureNavigation(null!));
        Assert.Throws<ArgumentNullException>(() => builder.ConfigureCustomHandler(null!));
        Assert.Throws<ArgumentNullException>(() => builder.ConfigureDynamicToolbar(null!));
        Assert.Throws<ArgumentNullException>(() => builder.ConfigureTips(null!));
        Assert.Throws<ArgumentNullException>(() => builder.ConfigureMotion(null!));
        Assert.Throws<ArgumentNullException>(() => builder.ConfigureWindow(null!));
        Assert.Throws<ArgumentNullException>(() => builder.ConfigureFooter(null!));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void ConfigureFont_WithMissingFamily_ThrowsArgumentException(string? fontFamily)
    {
        var builder = FlourishBuilder.CreateDefaultBuilder([]);

        var exception = Assert.Throws<ArgumentException>(() =>
            builder.ConfigureFont(fontFamily!)
        );

        Assert.Equal("fontFamily", exception.ParamName);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(double.NaN)]
    [InlineData(double.PositiveInfinity)]
    [InlineData(double.NegativeInfinity)]
    public void ConfigureFont_WithInvalidSize_ThrowsArgumentOutOfRangeException(double fontSize)
    {
        var builder = FlourishBuilder.CreateDefaultBuilder([]);

        var exception = Assert.Throws<ArgumentOutOfRangeException>(() =>
            builder.ConfigureFont("Segoe UI", fontSize)
        );

        Assert.Equal("fontSize", exception.ParamName);
    }

    [Fact]
    public void ConfigureMaterialEffect_WithUnknownValue_ThrowsArgumentOutOfRangeException()
    {
        var builder = FlourishBuilder.CreateDefaultBuilder([]);

        var exception = Assert.Throws<ArgumentOutOfRangeException>(() =>
            builder.ConfigureMaterialEffect((MaterialEffect)int.MaxValue)
        );

        Assert.Equal("effect", exception.ParamName);
    }

    [Fact]
    public void ConfigureThemes_WithUnknownValue_ThrowsArgumentOutOfRangeException()
    {
        var builder = FlourishBuilder.CreateDefaultBuilder([]);

        var exception = Assert.Throws<ArgumentOutOfRangeException>(() =>
            builder.ConfigureThemes((FlourishTheme)int.MaxValue)
        );

        Assert.Equal("defaultTheme", exception.ParamName);
    }

    [Fact]
    public void Build_AppliesConfigurationCallbacksAndPreservesRegistrationOrder()
    {
        var marker = new object();
        var builder = FlourishBuilder
            .CreateDefaultBuilder([])
            .ConfigureData(data => data.SetAppPreferenceDataPath(@"C:\Flourish.Test"))
            .ConfigureServices((_, services) => services.AddSingleton(marker))
            .ConfigureShell(shell => shell.UseNavigation().UseFooter())
            .ConfigureShell(shell => shell.UseFooter(enabled: false))
            .ConfigureTitleBar(titlebar => titlebar.SetTitle("Test Shell"))
            .ConfigureNavigation(navigation => navigation.SetTitle("Navigation"))
            .ConfigureCustomHandler(custom =>
                custom.Add(FlourishRegion.TitlebarStart, _ => null!)
            )
            .ConfigureDynamicToolbar(toolbar =>
                toolbar.CreateToolbarItems<TestPage>(
                    new FlourishToolbarItem("Refresh", "R", "test.refresh")
                )
            )
            .ConfigureTips(tips => tips.SetDelay(350))
            .ConfigureMotion(motion => motion.RespectSystemReducedMotion(enabled: false))
            .ConfigureWindow(window => window.UseTopmost())
            .ConfigureFont("Arial", 15)
            .ConfigureMaterialEffect(MaterialEffect.None)
            .ConfigureThemes(FlourishTheme.Dark)
            .ConfigureFooter(footer => footer.SetStatusText("Ready"));

        using var flourish = builder.Build();
        var options = flourish.GetRequiredService<FlourishShellOptions>();
        var dataOptions = flourish.GetRequiredService<FlourishDataOptions>();

        Assert.Same(marker, flourish.GetRequiredService<object>());
        Assert.Equal(@"C:\Flourish.Test", dataOptions.AppPreferenceDataPath);
        Assert.True(options.IsNavigationPanelEnabled);
        Assert.False(options.IsStatusBarEnabled);
        Assert.Equal("Test Shell", options.Title);
        Assert.Equal("Navigation", options.PaneTitle);
        Assert.Single(options.RegionContents);
        Assert.Single(options.DynamicToolbarItems[typeof(TestPage)]);
        Assert.Equal(350, options.Tips.InitialShowDelayMilliseconds);
        Assert.False(options.Motion.RespectSystemReducedMotion);
        Assert.True(options.WindowTopmost);
        Assert.Equal("Arial", options.FontFamily);
        Assert.Equal(15, options.FontSize);
        Assert.Equal(MaterialEffect.None, options.MaterialEffect);
        Assert.Equal(FlourishTheme.Dark, options.DefaultTheme);
        Assert.Equal("Ready", options.StatusText);
    }

    private sealed class TestPage : Page { }
}
