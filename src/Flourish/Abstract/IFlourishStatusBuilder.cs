namespace AcksheedSys.Flourish.Abstract;

public interface IFlourishStatusBuilder
{
    IFlourishStatusBuilder SetStatusText(string text);

    IFlourishStatusBuilder AddStatusItem(string displayText, string iconGlyph);

    IFlourishStatusBuilder ShowLANConnectionStatus();

    IFlourishStatusBuilder ShowPowerStatus();
}
