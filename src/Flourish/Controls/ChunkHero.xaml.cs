using System.Collections;
using System.Windows;
using System.Windows.Markup;
using WpfControl = System.Windows.Controls.Control;

namespace ArkheideSystem.Flourish.Controls;

/// <summary>Describes how a <see cref="ChunkHero" /> arranges its copy and presenter.</summary>
public enum ChunkHeroMode
{
    /// <summary>Places the text on the left and the presenter on the right.</summary>
    SplitLeft,

    /// <summary>Places the presenter on the left and the text on the right.</summary>
    SplitRight,

    /// <summary>Places the presenter behind the overlaid text.</summary>
    Overlay,
}

/// <summary>
/// Defines the leading page section with copy, action content, and a flexible visual presenter.
/// </summary>
[ContentProperty(nameof(ChunkHeroBody))]
public class ChunkHero : WpfControl
{
    /// <summary>Identifies the <see cref="ChunkHeroTitle" /> dependency property.</summary>
    public static readonly DependencyProperty ChunkHeroTitleProperty =
        DependencyProperty.Register(
            nameof(ChunkHeroTitle),
            typeof(string),
            typeof(ChunkHero),
            new FrameworkPropertyMetadata(string.Empty)
        );

    /// <summary>Identifies the <see cref="ChunkHeroDescription" /> dependency property.</summary>
    public static readonly DependencyProperty ChunkHeroDescriptionProperty =
        DependencyProperty.Register(
            nameof(ChunkHeroDescription),
            typeof(string),
            typeof(ChunkHero),
            new FrameworkPropertyMetadata(null)
        );

    /// <summary>Identifies the <see cref="ChunkHeroBody" /> dependency property.</summary>
    public static readonly DependencyProperty ChunkHeroBodyProperty =
        DependencyProperty.Register(
            nameof(ChunkHeroBody),
            typeof(object),
            typeof(ChunkHero),
            new FrameworkPropertyMetadata(null, OnLogicalContentChanged)
        );

    /// <summary>Identifies the <see cref="ChunkHeroMode" /> dependency property.</summary>
    public static readonly DependencyProperty ChunkHeroModeProperty =
        DependencyProperty.Register(
            nameof(ChunkHeroMode),
            typeof(ChunkHeroMode),
            typeof(ChunkHero),
            new FrameworkPropertyMetadata(
                global::ArkheideSystem.Flourish.Controls.ChunkHeroMode.SplitLeft
            ),
            IsChunkHeroModeValid
        );

    /// <summary>Identifies the <see cref="ChunkHeroPresenter" /> dependency property.</summary>
    public static readonly DependencyProperty ChunkHeroPresenterProperty =
        DependencyProperty.Register(
            nameof(ChunkHeroPresenter),
            typeof(object),
            typeof(ChunkHero),
            new FrameworkPropertyMetadata(null, OnLogicalContentChanged)
        );

    static ChunkHero()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(ChunkHero),
            new FrameworkPropertyMetadata(typeof(ChunkHero))
        );
    }

    /// <summary>Gets or sets the hero heading.</summary>
    public string ChunkHeroTitle
    {
        get => (string)GetValue(ChunkHeroTitleProperty);
        set => SetValue(ChunkHeroTitleProperty, value);
    }

    /// <summary>Gets or sets the optional supporting description.</summary>
    public string? ChunkHeroDescription
    {
        get => (string?)GetValue(ChunkHeroDescriptionProperty);
        set => SetValue(ChunkHeroDescriptionProperty, value);
    }

    /// <summary>Gets or sets action or supporting content displayed with the hero copy.</summary>
    public object? ChunkHeroBody
    {
        get => GetValue(ChunkHeroBodyProperty);
        set => SetValue(ChunkHeroBodyProperty, value);
    }

    /// <summary>Gets or sets how the hero copy and presenter are arranged.</summary>
    public ChunkHeroMode ChunkHeroMode
    {
        get => (ChunkHeroMode)GetValue(ChunkHeroModeProperty);
        set => SetValue(ChunkHeroModeProperty, value);
    }

    /// <summary>
    /// Gets or sets the visual content displayed beside or behind the hero copy. This can be an
    /// image, a color surface, or any other presentable object.
    /// </summary>
    public object? ChunkHeroPresenter
    {
        get => GetValue(ChunkHeroPresenterProperty);
        set => SetValue(ChunkHeroPresenterProperty, value);
    }

    /// <inheritdoc />
    protected override IEnumerator LogicalChildren => EnumerateLogicalChildren();

    private static bool IsChunkHeroModeValid(object value)
    {
        return value is ChunkHeroMode mode && Enum.IsDefined(mode);
    }

    private static void OnLogicalContentChanged(
        DependencyObject dependencyObject,
        DependencyPropertyChangedEventArgs eventArgs
    )
    {
        var hero = (ChunkHero)dependencyObject;
        if (eventArgs.OldValue is not null)
        {
            hero.RemoveLogicalChild(eventArgs.OldValue);
        }

        if (eventArgs.NewValue is not null)
        {
            hero.AddLogicalChild(eventArgs.NewValue);
        }
    }

    private IEnumerator EnumerateLogicalChildren()
    {
        if (ChunkHeroBody is not null)
        {
            yield return ChunkHeroBody;
        }

        if (ChunkHeroPresenter is not null)
        {
            yield return ChunkHeroPresenter;
        }
    }
}
