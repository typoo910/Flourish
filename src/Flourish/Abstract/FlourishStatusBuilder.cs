using Flourish.Models;

namespace AcksheedSys.Flourish.Abstract;

internal sealed class FlourishStatusBuilder : IFlourishStatusBuilder
{
    private readonly FlourishShellOptions options;

    public FlourishStatusBuilder(FlourishShellOptions options)
    {
        this.options = options;
    }

    public IFlourishStatusBuilder SetStatusText(string text)
    {
        options.StatusText = text;
        return this;
    }

    public IFlourishStatusBuilder AddStatusItem(string displayText, string iconGlyph)
    {
        options.StatusItems.Add(new FlourishStatusItem(displayText, iconGlyph));
        return this;
    }

    public IFlourishStatusBuilder ShowLANConnectionStatus()
    {
        var text = System.Net.NetworkInformation.NetworkInterface.GetIsNetworkAvailable()
            ? "已连接"
            : "未连接";

        return AddStatusItem(text, "\uE701");
    }

    public IFlourishStatusBuilder ShowPowerStatus()
    {
        return AddStatusItem("电源", "\uE850");
    }
}
