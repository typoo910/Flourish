using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using ArkheideSystem.Flourish.Abstract;
using ArkheideSystem.Flourish.Internal.Configuration;
using ArkheideSystem.Flourish.Services;

namespace ArkheideSystem.Flourish.Test.Services;

public sealed class FontServicePageTests
{
    [Fact]
    public void ApplyToPage_GlobalResourcesBridgeTheFrameInheritanceBoundary()
    {
        RunInSta(() =>
        {
            var service = new FontService(new FlourishShellOptions());
            var text = new TextBlock();
            var page = new FontPage { Content = text };
            page.Resources["FlourishFontFamily"] = new FontFamily("Arial");
            page.Resources["FlourishFontSizeStandard"] = 17d;

            service.ApplyToPage(page);

            Assert.Equal("Arial", page.FontFamily.Source);
            Assert.Equal("Arial", text.FontFamily.Source);
            Assert.Equal(17d, page.FontSize);
            Assert.Equal(17d, text.FontSize);
        });
    }

    [Fact]
    public void ApplyToPage_OverrideUpdatesInheritedAndResourceBasedTextButPreservesExplicitFont()
    {
        RunInSta(() =>
        {
            var service = new FontService(new FlourishShellOptions());
            var inheritedText = new TextBlock();
            var resourceText = new TextBlock();
            resourceText.SetResourceReference(
                TextBlock.FontFamilyProperty,
                "FlourishFontFamily"
            );
            var explicitText = new TextBlock { FontFamily = new FontFamily("Consolas") };
            var panel = new StackPanel();
            panel.Children.Add(inheritedText);
            panel.Children.Add(resourceText);
            panel.Children.Add(explicitText);
            var page = new FontPage { Content = panel };

            service.SetOverrideFont<FontPage>("Arial", 15, 18, 20, 21, 24, 30);
            service.ApplyToPage(page);

            Assert.Equal("Arial", page.FontFamily.Source);
            Assert.Equal("Arial", inheritedText.FontFamily.Source);
            Assert.Equal("Arial", resourceText.FontFamily.Source);
            Assert.Equal("Consolas", explicitText.FontFamily.Source);
            Assert.Equal(18d, page.FontSize);
            Assert.Equal(15d, page.Resources["FlourishFontSizeSmall"]);
            Assert.Equal(18d, page.Resources["FlourishFontSizeStandard"]);
            Assert.Equal(20d, page.Resources["FlourishFontSizeIcon"]);
            Assert.Equal(21d, page.Resources["FlourishFontSizeLarge"]);
            Assert.Equal(24d, page.Resources["FlourishFontSizeExtraLarge"]);
        });
    }

    [Fact]
    public void ApplyToPage_OverrideWithoutSizeFollowsGlobalAndRestoresOriginalResources()
    {
        RunInSta(() =>
        {
            var service = new FontService(new FlourishShellOptions());
            var page = new FontPage();
            var originalFamily = new FontFamily("Times New Roman");
            page.Resources["FlourishFontFamily"] = originalFamily;
            page.Resources["FlourishFontSizeSmall"] = 14d;
            page.Resources["FlourishFontSizeStandard"] = 15d;

            service.SetOverrideFont<FontPage>("Arial", 16, 20, 22, 24, 28, 34);
            service.ApplyToPage(page);
            Assert.Equal(16d, page.Resources["FlourishFontSizeSmall"]);
            Assert.Equal(20d, page.Resources["FlourishFontSizeStandard"]);
            Assert.Equal(22d, page.Resources["FlourishFontSizeIcon"]);
            Assert.Equal(24d, page.Resources["FlourishFontSizeLarge"]);
            Assert.Equal(28d, page.Resources["FlourishFontSizeExtraLarge"]);

            service.SetOverrideFont<FontPage>("Arial", null, null, null, null, null, null);
            service.ApplyToPage(page);
            Assert.Equal(15d, page.FontSize);
            Assert.Equal(14d, page.Resources["FlourishFontSizeSmall"]);
            Assert.Equal(15d, page.Resources["FlourishFontSizeStandard"]);
            Assert.False(page.Resources.Contains("FlourishFontSizeIcon"));
            Assert.False(page.Resources.Contains("FlourishFontSizeLarge"));
            Assert.False(page.Resources.Contains("FlourishFontSizeExtraLarge"));

            Assert.True(service.ClearOverrideFont<FontPage>());
            service.ApplyToPage(page);
            Assert.Same(originalFamily, page.Resources["FlourishFontFamily"]);
            Assert.Equal("Times New Roman", page.FontFamily.Source);
            Assert.Equal(15d, page.FontSize);
        });
    }

    [Fact]
    public void ApplyToPage_PartialOverrideKeepsExplicitTierAndRefreshesFollowingTiers()
    {
        RunInSta(() =>
        {
            var service = new FontService(new FlourishShellOptions());
            var resources = new ResourceDictionary();
            service.Attach(System.Windows.Threading.Dispatcher.CurrentDispatcher, resources);
            var page = new FontPage();
            page.Resources.MergedDictionaries.Add(resources);

            service.SetOverrideFont<FontPage>("Arial", 11, null, null, 19, null, null);

            Assert.True(service.ApplyToPage(page));
            Assert.Equal(11d, page.Resources["FlourishFontSizeSmall"]);
            Assert.Equal(14d, page.Resources["FlourishFontSizeStandard"]);
            Assert.Equal(16d, page.Resources["FlourishFontSizeIcon"]);
            Assert.Equal(19d, page.Resources["FlourishFontSizeLarge"]);
            Assert.Equal(24d, page.Resources["FlourishFontSizeExtraLarge"]);
            Assert.Equal(32d, page.Resources["FlourishFontSizeHeaderSize"]);

            service.SetFont("Segoe UI", 12, 16, 18, 21, 24, 30);

            Assert.True(service.ApplyToPage(page));
            Assert.Equal(11d, page.Resources["FlourishFontSizeSmall"]);
            Assert.Equal(16d, page.Resources["FlourishFontSizeStandard"]);
            Assert.Equal(18d, page.Resources["FlourishFontSizeIcon"]);
            Assert.Equal(19d, page.Resources["FlourishFontSizeLarge"]);
            Assert.Equal(24d, page.Resources["FlourishFontSizeExtraLarge"]);
            Assert.Equal(30d, page.Resources["FlourishFontSizeHeaderSize"]);
        });
    }

    [Fact]
    public void ApplyToPage_UsesConfiguredTypeForFactoryReturnedDerivedPage()
    {
        RunInSta(() =>
        {
            var service = new FontService(new FlourishShellOptions());
            var page = new DerivedFontPage();

            service.SetOverrideFont<FontPage>("Arial", 15, 19, 21, 23, 27, 33);
            service.ApplyToPage(page, typeof(FontPage));

            Assert.Equal("Arial", page.FontFamily.Source);
            Assert.Equal(19d, page.FontSize);
            Assert.Equal(15d, page.Resources["FlourishFontSizeSmall"]);
            Assert.Equal(21d, page.Resources["FlourishFontSizeIcon"]);
            Assert.Equal(23d, page.Resources["FlourishFontSizeLarge"]);
            Assert.Equal(27d, page.Resources["FlourishFontSizeExtraLarge"]);
        });
    }

    private static void RunInSta(Action action)
    {
        Exception? error = null;
        var thread = new Thread(() =>
        {
            try
            {
                action();
            }
            catch (Exception exception)
            {
                error = exception;
            }
        });
        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        thread.Join();

        if (error is not null)
        {
            System.Runtime.ExceptionServices.ExceptionDispatchInfo.Capture(error).Throw();
        }
    }

    private class FontPage : Page { }

    private sealed class DerivedFontPage : FontPage { }
}
