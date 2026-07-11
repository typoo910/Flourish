namespace ArkheideSystem.Flourish.Abstract;

/// <summary>
/// Provides a synchronous startup-time compatibility handler for command keys raised by Flourish UI surfaces.
/// </summary>
/// <remarks>
/// Use <see cref="ICommandRegistry" /> to add and remove handlers at runtime and
/// <see cref="ICommandDispatcher" /> to execute commands asynchronously. Implementations of this
/// interface registered during service configuration continue to run as ordered fallbacks.
/// </remarks>
/// <example>
/// <code><![CDATA[
/// services.AddSingleton<ICommandParser, AppCommandParser>();
/// ]]></code>
/// </example>
public interface ICommandParser
{
    /// <summary>
    /// Attempts to parse and handle a command key.
    /// </summary>
    /// <param name="commandKey">The command key associated with the requested action.</param>
    /// <returns><see langword="true" /> if the command was recognized and handled; otherwise, <see langword="false" />.</returns>
    /// <example>
    /// <code><![CDATA[
    /// public bool TryParse(string commandKey)
    /// {
    ///     return commandKey switch
    ///     {
    ///         "reports.export" => ExportReports(),
    ///         _ => false
    ///     };
    /// }
    /// ]]></code>
    /// </example>
    bool TryParse(string commandKey);
}
