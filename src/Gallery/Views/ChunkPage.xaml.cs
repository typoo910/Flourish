using System.Windows;
using System.Windows.Controls;
using ArkheideSystem.Flourish.Controls;

namespace ArkheideSystem.Gallery.Views;

public partial class ChunkPage : Page
{
    public ChunkPage()
    {
        InitializeComponent();
    }

    private void SplitLeft_Click(object sender, RoutedEventArgs e) =>
        HeroPreview.ChunkHeroMode = ChunkHeroMode.SplitLeft;

    private void SplitRight_Click(object sender, RoutedEventArgs e) =>
        HeroPreview.ChunkHeroMode = ChunkHeroMode.SplitRight;

    private void Overlay_Click(object sender, RoutedEventArgs e) =>
        HeroPreview.ChunkHeroMode = ChunkHeroMode.Overlay;
}
