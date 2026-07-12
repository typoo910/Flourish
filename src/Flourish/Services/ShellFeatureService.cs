using ArkheideSystem.Flourish.Abstract;
using ArkheideSystem.Flourish.Internal.Configuration;

namespace ArkheideSystem.Flourish.Services;

internal sealed class ShellFeatureService : IShellFeatureService
{
    private readonly object gate = new();
    private readonly FlourishShellOptions options;
    private readonly FlourishMotionService motionService;
    private long version;

    public ShellFeatureService(
        FlourishShellOptions options,
        FlourishMotionService motionService
    )
    {
        this.options = options ?? throw new ArgumentNullException(nameof(options));
        this.motionService =
            motionService ?? throw new ArgumentNullException(nameof(motionService));
        motionService.Changed += MotionService_Changed;
    }

    public event EventHandler<FlourishShellFeatureChangedEventArgs>? Changed;

    public FlourishShellFeatureState Current
    {
        get
        {
            lock (gate)
            {
                return CreateSnapshot();
            }
        }
    }

    public void SetEnabled(ShellFeature feature, bool enabled)
    {
        if (!Enum.IsDefined(feature))
        {
            throw new ArgumentOutOfRangeException(nameof(feature), feature, "Unknown shell feature.");
        }

        if (feature == ShellFeature.Motion)
        {
            motionService.SetEnabled(enabled);
            return;
        }

        FlourishShellFeatureState state;
        lock (gate)
        {
            if (CreateSnapshot().IsEnabled(feature) == enabled)
            {
                return;
            }

            switch (feature)
            {
                case ShellFeature.TitleBar:
                    options.IsTitlebarEnabled = enabled;
                    break;
                case ShellFeature.Navigation:
                    options.IsNavigationPanelEnabled = enabled;
                    break;
                case ShellFeature.DynamicToolbar:
                    options.IsDynamicToolbarEnabled = enabled;
                    break;
                case ShellFeature.StatusContent:
                    options.IsStatusBarEnabled = enabled;
                    break;
                case ShellFeature.ToolTips:
                    options.IsTipsEnabled = enabled;
                    break;
                case ShellFeature.Profile:
                    options.IsProfileEnabled = enabled;
                    break;
            }

            version++;
            state = CreateSnapshot();
        }

        Changed?.Invoke(this, new FlourishShellFeatureChangedEventArgs(feature, state));
    }

    private void MotionService_Changed(
        object? sender,
        FlourishMotionChangedEventArgs e
    )
    {
        if (e.Previous.IsEnabled == e.Current.IsEnabled)
        {
            return;
        }

        FlourishShellFeatureState state;
        lock (gate)
        {
            version++;
            state = CreateSnapshot(e.Current.IsEnabled);
        }

        Changed?.Invoke(
            this,
            new FlourishShellFeatureChangedEventArgs(ShellFeature.Motion, state)
        );
    }

    private FlourishShellFeatureState CreateSnapshot(bool? motionEnabled = null)
    {
        return new FlourishShellFeatureState(
            options.IsTitlebarEnabled,
            options.IsNavigationPanelEnabled,
            options.IsDynamicToolbarEnabled,
            options.IsStatusBarEnabled,
            options.IsTipsEnabled,
            motionEnabled ?? motionService.Current.IsEnabled,
            options.IsProfileEnabled,
            version
        );
    }
}
