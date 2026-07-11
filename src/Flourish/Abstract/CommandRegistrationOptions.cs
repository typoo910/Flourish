namespace ArkheideSystem.Flourish.Abstract;

/// <summary>
/// Configures a runtime command registration.
/// </summary>
public sealed class CommandRegistrationOptions
{
    /// <summary>
    /// Gets or initializes the policy applied when the command key is already registered.
    /// </summary>
    public CommandDuplicatePolicy DuplicatePolicy { get; init; } =
        CommandDuplicatePolicy.Reject;

    /// <summary>
    /// Gets or initializes the handler priority. Higher-priority handlers run first when duplicates are appended.
    /// </summary>
    public int Priority { get; init; }
}
