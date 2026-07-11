using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Shell;
using ArkheideSystem.Flourish.Abstract;
using ArkheideSystem.Flourish.Configuration;
using Brushes = System.Windows.Media.Brushes;
using Colors = System.Windows.Media.Colors;
using MediaBrush = System.Windows.Media.Brush;
using MediaColor = System.Windows.Media.Color;

namespace ArkheideSystem.Flourish.Services;

internal sealed class MaterialEffectService(FlourishShellOptions? options = null)
    : IMaterialEffectService
{
    private const int Succeeded = 0;
    private const int DwmwaUseImmersiveDarkMode = 20;
    private const int DwmwaSystemBackdropType = 38;
    private const int DwmsbtNone = 1;
    private const int DwmsbtMainWindow = 2;

    private readonly object stateGate = new();
    private Window? owner;
    private MediaBrush? originalBackground;
    private MediaColor originalCompositionBackground;
    private Thickness originalGlassFrameThickness;
    private bool hasOriginalCompositionBackground;
    private bool hasOriginalGlassFrameThickness;
    private bool isSourceInitializationPending;

    public MaterialEffect CurrentEffect { get; private set; } =
        options is { IsMaterialEffectEnabled: true }
            ? options.MaterialEffect
            : MaterialEffect.None;

    public bool IsApplied { get; private set; }

    public bool IsDarkMode { get; private set; }

    public event EventHandler<FlourishMaterialEffectChangedEventArgs>? Changed;

    public bool IsSupported(MaterialEffect effect)
    {
        ValidateEffect(effect, nameof(effect));
        return effect switch
        {
            MaterialEffect.None => true,
            MaterialEffect.Mica => IsSystemBackdropSupported(),
            _ => false,
        };
    }

    public void SetEffect(MaterialEffect effect)
    {
        ValidateEffect(effect, nameof(effect));
        bool effectChanged;
        lock (stateGate)
        {
            effectChanged = CurrentEffect != effect;
            CurrentEffect = effect;
            if (options is not null)
            {
                options.MaterialEffect = effect;
                options.IsMaterialEffectEnabled = effect != MaterialEffect.None;
            }
        }

        var attachedOwner = owner;
        if (attachedOwner is not null)
        {
            RunOnWindowDispatcher(attachedOwner, () => ApplyCurrentEffectCore(attachedOwner));
        }
        else
        {
            lock (stateGate)
            {
                IsApplied = false;
            }
        }

        if (effectChanged)
        {
            RaiseChanged();
        }
    }

    public void SetDarkMode(bool isDarkMode)
    {
        lock (stateGate)
        {
            if (IsDarkMode == isDarkMode)
            {
                return;
            }

            IsDarkMode = isDarkMode;
        }
        var attachedOwner = owner;
        if (attachedOwner is not null)
        {
            RunOnWindowDispatcher(attachedOwner, () =>
            {
                var hwnd = new WindowInteropHelper(attachedOwner).Handle;
                ApplyDarkMode(hwnd, isDarkMode);
            });
        }

        RaiseChanged();
    }

    internal void Attach(Window window, MaterialEffect effect)
    {
        ArgumentNullException.ThrowIfNull(window);
        ValidateEffect(effect, nameof(effect));

        if (owner is not null && owner != window && isSourceInitializationPending)
        {
            owner.SourceInitialized -= Owner_SourceInitialized;
            isSourceInitializationPending = false;
        }

        var ownerChanged = owner != window;
        owner = window;
        if (ownerChanged)
        {
            originalBackground = window.Background;
            hasOriginalCompositionBackground = false;
            hasOriginalGlassFrameThickness = false;
        }
        lock (stateGate)
        {
            CurrentEffect = effect;
        }

        if (new WindowInteropHelper(window).Handle != IntPtr.Zero)
        {
            ApplyCurrentEffectCore(window);
            return;
        }

        if (!isSourceInitializationPending)
        {
            isSourceInitializationPending = true;
            window.SourceInitialized += Owner_SourceInitialized;
        }
    }

    internal void Detach(Window window)
    {
        if (owner != window)
        {
            return;
        }

        if (isSourceInitializationPending)
        {
            window.SourceInitialized -= Owner_SourceInitialized;
            isSourceInitializationPending = false;
        }

        RunOnWindowDispatcher(window, () => RemoveEffectCore(window));
        owner = null;
    }

    internal void SetDarkMode(Window window, bool isDarkMode)
    {
        ArgumentNullException.ThrowIfNull(window);
        if (owner != window)
        {
            Attach(window, CurrentEffect);
        }

        SetDarkMode(isDarkMode);
    }

    private void Owner_SourceInitialized(object? sender, EventArgs e)
    {
        if (sender is not Window window || owner != window)
        {
            return;
        }

        window.SourceInitialized -= Owner_SourceInitialized;
        isSourceInitializationPending = false;
        ApplyCurrentEffectCore(window);
    }

    private void ApplyCurrentEffectCore(Window window)
    {
        if (CurrentEffect == MaterialEffect.None)
        {
            RemoveEffectCore(window);
            return;
        }

        if (!IsSupported(CurrentEffect))
        {
            RemoveEffectCore(window);
            return;
        }

        switch (CurrentEffect)
        {
            case MaterialEffect.Mica:
                ApplyMicaCore(window);
                break;
            case MaterialEffect.None:
                RemoveEffectCore(window);
                break;
            default:
                throw new ArgumentOutOfRangeException(
                    nameof(CurrentEffect),
                    CurrentEffect,
                    "Unknown material effect."
                );
        }
    }

    private void ApplyMicaCore(Window window)
    {
        var hwnd = new WindowInteropHelper(window).Handle;
        if (hwnd == IntPtr.Zero)
        {
            lock (stateGate)
            {
                IsApplied = false;
            }
            return;
        }

        window.Background = Brushes.Transparent;
        if (HwndSource.FromHwnd(hwnd) is { CompositionTarget: { } compositionTarget })
        {
            if (!hasOriginalCompositionBackground)
            {
                originalCompositionBackground = compositionTarget.BackgroundColor;
                hasOriginalCompositionBackground = true;
            }

            compositionTarget.BackgroundColor = Colors.Transparent;
        }

        if (WindowChrome.GetWindowChrome(window) is { } chrome)
        {
            if (!hasOriginalGlassFrameThickness)
            {
                originalGlassFrameThickness = chrome.GlassFrameThickness;
                hasOriginalGlassFrameThickness = true;
            }

            chrome.GlassFrameThickness = new Thickness(-1);
        }

        var backdropType = DwmsbtMainWindow;
        var isApplied =
            DwmSetWindowAttribute(
                hwnd,
                DwmwaSystemBackdropType,
                ref backdropType,
                Marshal.SizeOf<int>()
            ) == Succeeded;
        lock (stateGate)
        {
            IsApplied = isApplied;
        }
        ApplyDarkMode(hwnd, IsDarkMode);
    }

    private void RemoveEffectCore(Window window)
    {
        var hwnd = new WindowInteropHelper(window).Handle;
        if (hwnd != IntPtr.Zero && IsSystemBackdropSupported())
        {
            var backdropType = DwmsbtNone;
            DwmSetWindowAttribute(
                hwnd,
                DwmwaSystemBackdropType,
                ref backdropType,
                Marshal.SizeOf<int>()
            );
        }

        window.Background = originalBackground;
        if (
            hwnd != IntPtr.Zero
            && hasOriginalCompositionBackground
            && HwndSource.FromHwnd(hwnd) is { CompositionTarget: { } compositionTarget }
        )
        {
            compositionTarget.BackgroundColor = originalCompositionBackground;
        }

        if (
            hasOriginalGlassFrameThickness
            && WindowChrome.GetWindowChrome(window) is { } chrome
        )
        {
            chrome.GlassFrameThickness = originalGlassFrameThickness;
        }

        lock (stateGate)
        {
            IsApplied = false;
        }
        ApplyDarkMode(hwnd, IsDarkMode);
    }

    private void RaiseChanged()
    {
        void RaiseCore()
        {
            MaterialEffect effect;
            bool isApplied;
            bool isDarkMode;
            lock (stateGate)
            {
                effect = CurrentEffect;
                isApplied = IsApplied;
                isDarkMode = IsDarkMode;
            }

            Changed?.Invoke(
                this,
                new FlourishMaterialEffectChangedEventArgs(
                    effect,
                    IsSupported(effect),
                    isApplied,
                    isDarkMode
                )
            );
        }

        var attachedOwner = owner;
        if (attachedOwner is null || attachedOwner.Dispatcher.CheckAccess())
        {
            RaiseCore();
            return;
        }

        attachedOwner.Dispatcher.Invoke(RaiseCore);
    }

    private static void ApplyDarkMode(IntPtr hwnd, bool isDarkMode)
    {
        if (hwnd == IntPtr.Zero || !IsImmersiveDarkModeSupported())
        {
            return;
        }

        var darkMode = isDarkMode ? 1 : 0;
        DwmSetWindowAttribute(
            hwnd,
            DwmwaUseImmersiveDarkMode,
            ref darkMode,
            Marshal.SizeOf<int>()
        );
    }

    private static bool IsSystemBackdropSupported()
    {
        return OperatingSystem.IsWindowsVersionAtLeast(10, 0, 22621);
    }

    private static bool IsImmersiveDarkModeSupported()
    {
        return OperatingSystem.IsWindowsVersionAtLeast(10, 0, 22000);
    }

    private static void ValidateEffect(MaterialEffect effect, string parameterName)
    {
        if (!Enum.IsDefined(effect))
        {
            throw new ArgumentOutOfRangeException(
                parameterName,
                effect,
                "Unknown material effect."
            );
        }
    }

    private static void RunOnWindowDispatcher(Window window, Action action)
    {
        if (window.Dispatcher.CheckAccess())
        {
            action();
            return;
        }

        window.Dispatcher.Invoke(action);
    }

    [DllImport("dwmapi.dll", PreserveSig = true)]
    private static extern int DwmSetWindowAttribute(
        IntPtr hwnd,
        int dwAttribute,
        ref int pvAttribute,
        int cbAttribute
    );
}
