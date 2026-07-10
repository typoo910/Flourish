using ArkheideSystem.Flourish.Configuration;
using ArkheideSystem.Flourish.Composition;

namespace ArkheideSystem.Flourish.Test.Composition;

public sealed class FlourishShellBuilderTests
{
    [Fact]
    public void FeatureMethods_WithDefaultValues_EnableAllFeaturesAndReturnBuilder()
    {
        var options = new FlourishShellOptions();
        var sut = new FlourishShellBuilder(options);

        Assert.Same(sut, sut.UseTitleBar());
        Assert.Same(sut, sut.UseNavigation());
        Assert.Same(sut, sut.UseDynamicToolbar());
        Assert.Same(sut, sut.UseTips());
        Assert.Same(sut, sut.UseMotion());
        Assert.Same(sut, sut.UseMaterialEffect());
        Assert.Same(sut, sut.UseThemes());
        Assert.Same(sut, sut.UseFooter());

        Assert.True(options.IsTitlebarEnabled);
        Assert.True(options.IsNavigationPanelEnabled);
        Assert.True(options.IsDynamicToolbarEnabled);
        Assert.True(options.IsTipsEnabled);
        Assert.True(options.Motion.IsEnabled);
        Assert.True(options.IsMaterialEffectEnabled);
        Assert.True(options.IsThemeEnabled);
        Assert.True(options.IsStatusBarEnabled);
    }

    [Fact]
    public void FeatureMethods_WithFalse_DisableAllFeaturesAndReturnBuilder()
    {
        var options = new FlourishShellOptions
        {
            IsTitlebarEnabled = true,
            IsNavigationPanelEnabled = true,
            IsDynamicToolbarEnabled = true,
            IsTipsEnabled = true,
            IsMaterialEffectEnabled = true,
            IsThemeEnabled = true,
            IsStatusBarEnabled = true,
        };
        options.Motion.IsEnabled = true;
        var sut = new FlourishShellBuilder(options);

        Assert.Same(sut, sut.UseTitleBar(false));
        Assert.Same(sut, sut.UseNavigation(false));
        Assert.Same(sut, sut.UseDynamicToolbar(false));
        Assert.Same(sut, sut.UseTips(false));
        Assert.Same(sut, sut.UseMotion(false));
        Assert.Same(sut, sut.UseMaterialEffect(false));
        Assert.Same(sut, sut.UseThemes(false));
        Assert.Same(sut, sut.UseFooter(false));

        Assert.False(options.IsTitlebarEnabled);
        Assert.False(options.IsNavigationPanelEnabled);
        Assert.False(options.IsDynamicToolbarEnabled);
        Assert.False(options.IsTipsEnabled);
        Assert.False(options.Motion.IsEnabled);
        Assert.False(options.IsMaterialEffectEnabled);
        Assert.False(options.IsThemeEnabled);
        Assert.False(options.IsStatusBarEnabled);
    }
}
