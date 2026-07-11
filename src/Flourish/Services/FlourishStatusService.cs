using ArkheideSystem.Flourish.Configuration;

namespace ArkheideSystem.Flourish.Services;

internal sealed class FlourishStatusService(FlourishShellOptions options)
{
    public IReadOnlyList<FlourishStatusItem> StatusItems => options.StatusItems;

    public bool IsLANConnectionStatusEnabled => options.IsLANConnectionStatusEnabled;

    public bool IsPowerStatusEnabled => options.IsPowerStatusEnabled;
}
