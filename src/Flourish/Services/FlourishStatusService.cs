using AcksheedSys.Flourish.Abstract;
using AcksheedSys.Flourish.Configuration;

namespace AcksheedSys.Flourish.Services;

internal sealed class FlourishStatusService(FlourishShellOptions options)
{
    public string StatusText => options.StatusText;

    public IReadOnlyList<FlourishStatusItem> StatusItems => options.StatusItems;
}
