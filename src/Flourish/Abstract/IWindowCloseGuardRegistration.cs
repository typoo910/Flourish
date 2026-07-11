namespace ArkheideSystem.Flourish.Abstract;

/// <summary>
/// Represents a runtime window close guard registration.
/// </summary>
public interface IWindowCloseGuardRegistration : IDisposable
{
    /// <summary>
    /// Gets the unique identifier supplied when the guard was registered.
    /// </summary>
    string Id { get; }
}
