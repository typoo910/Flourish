using ArkheideSystem.Flourish.Abstract;
using ArkheideSystem.Flourish.Configuration;
using Application = System.Windows.Application;
using FontFamily = System.Windows.Media.FontFamily;
using Window = System.Windows.Window;

namespace ArkheideSystem.Flourish.Services;

internal sealed class FontService(FlourishShellOptions options) : IFontService
{
    private readonly object gate = new();
    private Window? owner;

    public string FontFamily
    {
        get
        {
            lock (gate)
            {
                return options.FontFamily;
            }
        }
    }

    public string IconFontFamily
    {
        get
        {
            lock (gate)
            {
                return options.IconFontFamily;
            }
        }
    }

    public double FontSize
    {
        get
        {
            lock (gate)
            {
                return options.FontSize;
            }
        }
    }

    public event EventHandler<FlourishFontChangedEventArgs>? Changed;

    public void Apply(Window window)
    {
        owner = window;
        ApplyCore(window);
    }

    public void SetFont(string fontFamily, double fontSize)
    {
        fontFamily = ValidateNotBlank(fontFamily, nameof(fontFamily));
        ValidatePositiveFinite(fontSize, nameof(fontSize));
        lock (gate)
        {
            if (options.FontFamily == fontFamily && options.FontSize == fontSize)
            {
                return;
            }

            options.FontFamily = fontFamily;
            options.FontSize = fontSize;
        }

        ApplyAndNotify();
    }

    public void SetFontFamily(string fontFamily)
    {
        fontFamily = ValidateNotBlank(fontFamily, nameof(fontFamily));
        lock (gate)
        {
            if (options.FontFamily == fontFamily)
            {
                return;
            }

            options.FontFamily = fontFamily;
        }

        ApplyAndNotify();
    }

    public void SetFontSize(double fontSize)
    {
        ValidatePositiveFinite(fontSize, nameof(fontSize));
        lock (gate)
        {
            if (options.FontSize == fontSize)
            {
                return;
            }

            options.FontSize = fontSize;
        }

        ApplyAndNotify();
    }

    public void SetIconFontFamily(string fontFamily)
    {
        fontFamily = ValidateNotBlank(fontFamily, nameof(fontFamily));
        lock (gate)
        {
            if (options.IconFontFamily == fontFamily)
            {
                return;
            }

            options.IconFontFamily = fontFamily;
        }

        ApplyAndNotify();
    }

    private void ApplyAndNotify()
    {
        var attachedOwner = owner;
        if (attachedOwner is not null)
        {
            if (attachedOwner.CheckAccess())
            {
                ApplyCore(attachedOwner);
                RaiseChanged();
            }
            else
            {
                attachedOwner.Dispatcher.Invoke(() =>
                {
                    ApplyCore(attachedOwner);
                    RaiseChanged();
                });
            }

            return;
        }

        RaiseChanged();
    }

    private void RaiseChanged()
    {
        Changed?.Invoke(
            this,
            new FlourishFontChangedEventArgs(FontFamily, IconFontFamily, FontSize)
        );
    }

    private void ApplyCore(Window window)
    {
        string textFontFamily;
        string iconFontFamilyName;
        double baseSize;
        lock (gate)
        {
            textFontFamily = options.FontFamily;
            iconFontFamilyName = options.IconFontFamily;
            baseSize = options.FontSize;
        }

        var fontFamily = new FontFamily(textFontFamily);
        var iconFontFamily = new FontFamily(iconFontFamilyName);

        window.FontFamily = fontFamily;
        window.FontSize = baseSize;

        SetResource(window, "FlourishFontFamily", fontFamily);
        SetResource(window, "FlourishIconFontFamily", iconFontFamily);
        SetResource(window, "FlourishFontSizeSmall", ClampFontSize(baseSize - 3));
        SetResource(window, "FlourishFontSizeCaption", ClampFontSize(baseSize - 1));
        SetResource(window, "FlourishFontSizeBase", baseSize);
        SetResource(window, "FlourishFontSizeTitle", baseSize);
        SetResource(window, "FlourishFontSizeTitlebarIcon", ClampFontSize(baseSize + 1));
        SetResource(window, "FlourishFontSizeNavigationIcon", ClampFontSize(baseSize + 1));
        SetResource(window, "FlourishFontSizeWindowButtonIcon", ClampFontSize(baseSize - 2));
    }

    private static void SetResource(Window window, string key, object value)
    {
        window.Resources[key] = value;
        Application.Current?.Resources[key] = value;
    }

    private static double ClampFontSize(double size)
    {
        return Math.Max(1, size);
    }

    private static string ValidateNotBlank(string value, string parameterName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Value cannot be empty.", parameterName);
        }

        return value;
    }

    private static void ValidatePositiveFinite(double value, string parameterName)
    {
        if (double.IsNaN(value) || double.IsInfinity(value) || value <= 0)
        {
            throw new ArgumentOutOfRangeException(
                parameterName,
                value,
                "Value must be a positive finite number."
            );
        }
    }
}
