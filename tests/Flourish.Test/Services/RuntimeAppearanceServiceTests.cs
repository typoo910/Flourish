using System.IO;
using ArkheideSystem.Flourish.Abstract;
using ArkheideSystem.Flourish.Configuration;
using ArkheideSystem.Flourish.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Moq;

namespace ArkheideSystem.Flourish.Test.Services;

public sealed class RuntimeAppearanceServiceTests
{
    [Fact]
    public void FontService_UpdatesSettingsAndRaisesChanged()
    {
        var options = new FlourishShellOptions();
        IFontService sut = new FontService(options);
        FlourishFontChangedEventArgs? change = null;
        sut.Changed += (_, args) => change = args;

        sut.SetFont("Arial", 16);
        sut.SetIconFontFamily("Segoe Fluent Icons");

        Assert.Equal("Arial", sut.FontFamily);
        Assert.Equal(16, sut.FontSize);
        Assert.Equal("Segoe Fluent Icons", sut.IconFontFamily);
        Assert.NotNull(change);
        Assert.Equal("Segoe Fluent Icons", change.IconFontFamily);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(double.NaN)]
    [InlineData(double.PositiveInfinity)]
    public void FontService_InvalidSize_Throws(double size)
    {
        IFontService sut = new FontService(new FlourishShellOptions());

        Assert.Throws<ArgumentOutOfRangeException>(() => sut.SetFontSize(size));
    }

    [Fact]
    public void ToolTipService_EnablesConfiguresAndDisablesAtRuntime()
    {
        var options = new FlourishShellOptions();
        IToolTipService sut = new FlourishToolTipService(options);
        var changes = new List<FlourishToolTipChangedEventArgs>();
        sut.Changed += (_, args) => changes.Add(args);

        sut.Configure(450, 8);
        sut.SetEnabled(false);

        Assert.False(sut.Current.IsEnabled);
        Assert.Equal(450, sut.Current.InitialShowDelayMilliseconds);
        Assert.Equal(8, sut.Current.SpawnableMargin);
        Assert.Equal(2, changes.Count);
        Assert.True(changes[0].Current.IsEnabled);
        Assert.False(changes[1].Current.IsEnabled);
    }

    [Theory]
    [InlineData(-1, 5)]
    [InlineData(0, -1)]
    [InlineData(0, double.NaN)]
    [InlineData(0, double.PositiveInfinity)]
    public void ToolTipService_InvalidSettings_Throw(int delay, double margin)
    {
        IToolTipService sut = new FlourishToolTipService(new FlourishShellOptions());

        Assert.Throws<ArgumentOutOfRangeException>(() => sut.Configure(delay, margin));
    }

    [Fact]
    public void MotionService_UpdatesIndependentRuntimeSettings()
    {
        IMotionService sut = new FlourishMotionService(new FlourishShellOptions());
        var changes = new List<FlourishMotionChangedEventArgs>();
        sut.Changed += (_, args) => changes.Add(args);

        sut.SetEnabled(true);
        sut.SetPageTransition(FlourishPageTransition.Fade, TimeSpan.FromMilliseconds(250));
        sut.SetNavigationPanelTransition(
            FlourishNavigationPanelTransition.None,
            TimeSpan.FromMilliseconds(100)
        );
        sut.SetHoverReveal(true, TimeSpan.FromMilliseconds(90));
        sut.SetRespectSystemReducedMotion(false);

        Assert.True(sut.Current.IsEnabled);
        Assert.Equal(FlourishPageTransition.Fade, sut.Current.PageTransition);
        Assert.Equal(TimeSpan.FromMilliseconds(250), sut.Current.PageTransitionDuration);
        Assert.Equal(
            FlourishNavigationPanelTransition.None,
            sut.Current.NavigationPanelTransition
        );
        Assert.True(sut.Current.IsHoverRevealEnabled);
        Assert.False(sut.Current.RespectSystemReducedMotion);
        Assert.True(sut.CanAnimate);
        Assert.Equal(5, changes.Count);
    }

    [Fact]
    public void MotionService_InvalidTransitionValues_Throw()
    {
        IMotionService sut = new FlourishMotionService(new FlourishShellOptions());

        Assert.Throws<ArgumentOutOfRangeException>(() =>
            sut.SetPageTransition(
                (FlourishPageTransition)int.MaxValue,
                TimeSpan.FromMilliseconds(1)
            )
        );
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            sut.SetHoverReveal(true, TimeSpan.Zero)
        );
    }

    [Fact]
    public void MaterialEffectService_TracksRequestedRuntimeStateWithoutOwner()
    {
        IMaterialEffectService sut = new MaterialEffectService();
        var changes = new List<FlourishMaterialEffectChangedEventArgs>();
        sut.Changed += (_, args) => changes.Add(args);

        sut.SetEffect(MaterialEffect.Mica);
        sut.SetDarkMode(true);

        Assert.Equal(MaterialEffect.Mica, sut.CurrentEffect);
        Assert.False(sut.IsApplied);
        Assert.True(sut.IsDarkMode);
        Assert.Equal(2, changes.Count);
        Assert.Equal(sut.IsSupported(MaterialEffect.Mica), changes[0].IsSupported);
    }

    [Fact]
    public void ThemeService_SetThemeActivatesRuntimeThemeAndPersistsIt()
    {
        using var directory = new TemporaryDirectory();
        var configuration = new ConfigurationBuilder()
            .SetBasePath(directory.Path)
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: false)
            .Build();
        var environment = new Mock<IHostEnvironment>();
        environment.SetupGet(value => value.ContentRootPath).Returns(directory.Path);
        var preferences = new AppPreferenceService(configuration, environment.Object);
        var options = new FlourishShellOptions();
        IThemeService sut = new ThemeService(options, preferences);
        FlourishThemeChangedEventArgs? change = null;
        sut.ThemeChanged += (_, args) => change = args;

        sut.SetTheme(FlourishTheme.Dark);

        Assert.Equal(FlourishTheme.Dark, sut.CurrentTheme);
        Assert.Equal(FlourishTheme.Dark, sut.EffectiveTheme);
        Assert.True(sut.IsDark);
        Assert.True(options.IsThemeEnabled);
        Assert.Equal(FlourishTheme.Dark, preferences.ReadTheme());
        Assert.NotNull(change);
        Assert.Equal(FlourishTheme.Dark, change.RequestedTheme);
    }

    private sealed class TemporaryDirectory : IDisposable
    {
        public TemporaryDirectory()
        {
            Path = System.IO.Path.Combine(
                System.IO.Path.GetTempPath(),
                "Flourish.Test",
                Guid.NewGuid().ToString("N")
            );
            Directory.CreateDirectory(Path);
        }

        public string Path { get; }

        public void Dispose()
        {
            if (Directory.Exists(Path))
            {
                Directory.Delete(Path, recursive: true);
            }
        }
    }
}
