namespace ArkheideSystem.Flourish.Abstract;

/// <summary>
/// Specifies how runtime command registration handles an existing command key.
/// </summary>
public enum CommandDuplicatePolicy
{
    /// <summary>
    /// Reject the new registration by throwing an exception.
    /// </summary>
    Reject = 0,

    /// <summary>
    /// Remove all existing runtime registrations for the command key before registering the new handler.
    /// </summary>
    Replace,

    /// <summary>
    /// Keep the existing handlers and append the new handler.
    /// </summary>
    Append,
}
