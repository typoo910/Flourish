using System.Windows.Media;
using ArkheideSystem.Flourish.Abstract;
using ArkheideSystem.Flourish.Configuration;
using ArkheideSystem.Flourish.Composition;

namespace ArkheideSystem.Flourish.Test.Composition;

public sealed class FlourishTitlebarBuilderTests
{
    [Fact]
    public void VisibilityMethods_WithDefaultValues_EnableAllElementsAndReturnBuilder()
    {
        var options = new FlourishShellOptions();
        var sut = new FlourishTitlebarBuilder(options);

        Assert.Same(sut, sut.ShowSearch());
        Assert.Same(sut, sut.ShowBreadcrumb());
        Assert.Same(sut, sut.ShowNavToggle());
        Assert.Same(sut, sut.ShowLogo());
        Assert.Same(sut, sut.ShowTitle());
        Assert.Same(sut, sut.ShowSubTitle());
        Assert.Same(sut, sut.ShowProfile());
        Assert.Same(sut, sut.ShowThemeToggle());

        Assert.True(options.IsTitlebarSearchEnabled);
        Assert.True(options.IsBreadcrumbEnabled);
        Assert.True(options.IsTitlebarNavigationToggleEnabled);
        Assert.True(options.IsTitlebarLogoEnabled);
        Assert.True(options.IsTitlebarTitleEnabled);
        Assert.True(options.IsTitlebarSubtitleEnabled);
        Assert.True(options.IsTitlebarProfileEnabled);
        Assert.True(options.IsTitlebarThemeToggleEnabled);
    }

    [Fact]
    public void VisibilityMethods_WithFalse_DisableAllElementsAndReturnBuilder()
    {
        var options = new FlourishShellOptions
        {
            IsTitlebarSearchEnabled = true,
            IsBreadcrumbEnabled = true,
            IsTitlebarNavigationToggleEnabled = true,
            IsTitlebarLogoEnabled = true,
            IsTitlebarTitleEnabled = true,
            IsTitlebarSubtitleEnabled = true,
            IsTitlebarProfileEnabled = true,
            IsTitlebarThemeToggleEnabled = true,
        };
        var sut = new FlourishTitlebarBuilder(options);

        Assert.Same(sut, sut.ShowSearch(false));
        Assert.Same(sut, sut.ShowBreadcrumb(false));
        Assert.Same(sut, sut.ShowNavToggle(false));
        Assert.Same(sut, sut.ShowLogo(false));
        Assert.Same(sut, sut.ShowTitle(false));
        Assert.Same(sut, sut.ShowSubTitle(false));
        Assert.Same(sut, sut.ShowProfile(false));
        Assert.Same(sut, sut.ShowThemeToggle(false));

        Assert.False(options.IsTitlebarSearchEnabled);
        Assert.False(options.IsBreadcrumbEnabled);
        Assert.False(options.IsTitlebarNavigationToggleEnabled);
        Assert.False(options.IsTitlebarLogoEnabled);
        Assert.False(options.IsTitlebarTitleEnabled);
        Assert.False(options.IsTitlebarSubtitleEnabled);
        Assert.False(options.IsTitlebarProfileEnabled);
        Assert.False(options.IsTitlebarThemeToggleEnabled);
    }

    [Fact]
    public void TextAndBehaviorMethods_UpdateOptionsAndReturnBuilder()
    {
        var options = new FlourishShellOptions();
        var sut = new FlourishTitlebarBuilder(options);

        Assert.Same(sut, sut.SetTrayExit(true));
        Assert.Same(sut, sut.SetTitle("Gallery"));
        Assert.Same(sut, sut.SetSubtitle("Sample application"));
        Assert.Same(sut, sut.SetLogoFallbackText("G"));
        Assert.Same(sut, sut.SetSearchPlaceholder("Search pages"));
        Assert.Same(sut, sut.SetBreadcrumbBehavior(BreadcrumbShowOption.Always));

        Assert.True(options.IsTrayExitEnabled);
        Assert.Equal("Gallery", options.Title);
        Assert.Equal("Sample application", options.Subtitle);
        Assert.True(options.IsTitlebarLogoEnabled);
        Assert.Equal("G", options.LogoFallbackText);
        Assert.Equal("Search pages", options.SearchPlaceholder);
        Assert.Equal(BreadcrumbShowOption.Always, options.BreadcrumbShowOption);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void SetTitle_WithBlankValue_ThrowsArgumentException(string? title)
    {
        var sut = new FlourishTitlebarBuilder(new FlourishShellOptions());

        var exception = Assert.Throws<ArgumentException>(() => sut.SetTitle(title!));

        Assert.Equal("title", exception.ParamName);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void SetLogoFallbackText_WithBlankValue_ThrowsArgumentException(string? text)
    {
        var sut = new FlourishTitlebarBuilder(new FlourishShellOptions());

        var exception = Assert.Throws<ArgumentException>(() =>
            sut.SetLogoFallbackText(text!)
        );

        Assert.Equal("fallbackText", exception.ParamName);
    }

    [Fact]
    public void SetLogo_WithNullImageSource_ThrowsArgumentNullException()
    {
        var sut = new FlourishTitlebarBuilder(new FlourishShellOptions());

        var exception = Assert.Throws<ArgumentNullException>(() =>
            sut.SetLogo((ImageSource)null!)
        );

        Assert.Equal("logoSource", exception.ParamName);
    }

    [Fact]
    public void SetSearchHandler_WithTextCallback_EnablesSearchAndForwardsText()
    {
        var options = new FlourishShellOptions();
        var sut = new FlourishTitlebarBuilder(options);
        string? receivedText = null;

        var result = sut.SetSearchHandler(text => receivedText = text);
        options.TitlebarSearchTextChanged!(new EmptyServiceProvider(), "flourish");

        Assert.Same(sut, result);
        Assert.True(options.IsTitlebarSearchEnabled);
        Assert.Equal("flourish", receivedText);
    }

    [Fact]
    public void SetSearchHandler_WithServiceCallback_EnablesSearchAndPreservesCallback()
    {
        var options = new FlourishShellOptions();
        var sut = new FlourishTitlebarBuilder(options);
        IServiceProvider? receivedServices = null;
        string? receivedText = null;
        Action<IServiceProvider, string> handler = (services, text) =>
        {
            receivedServices = services;
            receivedText = text;
        };
        var serviceProvider = new EmptyServiceProvider();

        var result = sut.SetSearchHandler(handler);
        options.TitlebarSearchTextChanged!(serviceProvider, "query");

        Assert.Same(sut, result);
        Assert.True(options.IsTitlebarSearchEnabled);
        Assert.Same(handler, options.TitlebarSearchTextChanged);
        Assert.Same(serviceProvider, receivedServices);
        Assert.Equal("query", receivedText);
    }

    [Fact]
    public void SetSearchHandler_WithNullTextCallback_ThrowsArgumentNullException()
    {
        var sut = new FlourishTitlebarBuilder(new FlourishShellOptions());

        var exception = Assert.Throws<ArgumentNullException>(() =>
            sut.SetSearchHandler((Action<string>)null!)
        );

        Assert.Equal("searchTextChanged", exception.ParamName);
    }

    [Fact]
    public void SetSearchHandler_WithNullServiceCallback_ThrowsArgumentNullException()
    {
        var sut = new FlourishTitlebarBuilder(new FlourishShellOptions());

        var exception = Assert.Throws<ArgumentNullException>(() =>
            sut.SetSearchHandler((Action<IServiceProvider, string>)null!)
        );

        Assert.Equal("searchTextChanged", exception.ParamName);
    }

    [Fact]
    public void SetBreadcrumbBehavior_WithUndefinedValue_ThrowsArgumentOutOfRangeException()
    {
        var sut = new FlourishTitlebarBuilder(new FlourishShellOptions());

        var exception = Assert.Throws<ArgumentOutOfRangeException>(() =>
            sut.SetBreadcrumbBehavior((BreadcrumbShowOption)int.MaxValue)
        );

        Assert.Equal("behavior", exception.ParamName);
    }

    private sealed class EmptyServiceProvider : IServiceProvider
    {
        public object? GetService(Type serviceType) => null;
    }
}
