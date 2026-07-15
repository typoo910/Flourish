using System.Collections;
using System.Windows;
using System.Windows.Markup;
using WpfControl = System.Windows.Controls.Control;

namespace ArkheideSystem.Flourish.Controls;

/// <summary>
/// Defines a consistently spaced page section with a title, optional description,
/// and body content.
/// </summary>
[ContentProperty(nameof(ChunkBody))]
public class Chunk : WpfControl
{
    private static readonly Thickness DefaultChunkMargin = new(0, 32, 0, 0);
    private static readonly Thickness DefaultChunkSpacing = new(0, 12, 0, 0);

    /// <summary>Identifies the <see cref="ChunkTitle" /> dependency property.</summary>
    public static readonly DependencyProperty ChunkTitleProperty = DependencyProperty.Register(
        nameof(ChunkTitle),
        typeof(string),
        typeof(Chunk),
        new FrameworkPropertyMetadata(string.Empty)
    );

    /// <summary>Identifies the <see cref="ChunkDescription" /> dependency property.</summary>
    public static readonly DependencyProperty ChunkDescriptionProperty =
        DependencyProperty.Register(
            nameof(ChunkDescription),
            typeof(string),
            typeof(Chunk),
            new FrameworkPropertyMetadata(null)
        );

    /// <summary>Identifies the <see cref="ChunkMargin" /> dependency property.</summary>
    public static readonly DependencyProperty ChunkMarginProperty = DependencyProperty.Register(
        nameof(ChunkMargin),
        typeof(Thickness),
        typeof(Chunk),
        new FrameworkPropertyMetadata(DefaultChunkMargin)
    );

    /// <summary>Identifies the <see cref="ChunkSpacing" /> dependency property.</summary>
    public static readonly DependencyProperty ChunkSpacingProperty =
        DependencyProperty.Register(
            nameof(ChunkSpacing),
            typeof(Thickness),
            typeof(Chunk),
            new FrameworkPropertyMetadata(DefaultChunkSpacing)
        );

    /// <summary>Identifies the <see cref="ChunkBody" /> dependency property.</summary>
    public static readonly DependencyProperty ChunkBodyProperty = DependencyProperty.Register(
        nameof(ChunkBody),
        typeof(object),
        typeof(Chunk),
        new FrameworkPropertyMetadata(null, OnChunkBodyChanged)
    );

    static Chunk()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(Chunk),
            new FrameworkPropertyMetadata(typeof(Chunk))
        );
    }

    /// <summary>Gets or sets the section heading.</summary>
    public string ChunkTitle
    {
        get => (string)GetValue(ChunkTitleProperty);
        set => SetValue(ChunkTitleProperty, value);
    }

    /// <summary>Gets or sets the optional supporting description.</summary>
    public string? ChunkDescription
    {
        get => (string?)GetValue(ChunkDescriptionProperty);
        set => SetValue(ChunkDescriptionProperty, value);
    }

    /// <summary>Gets or sets the space reserved after this section.</summary>
    public Thickness ChunkMargin
    {
        get => (Thickness)GetValue(ChunkMarginProperty);
        set => SetValue(ChunkMarginProperty, value);
    }

    /// <summary>Gets or sets the spacing between the section header elements and body.</summary>
    public Thickness ChunkSpacing
    {
        get => (Thickness)GetValue(ChunkSpacingProperty);
        set => SetValue(ChunkSpacingProperty, value);
    }

    /// <summary>Gets or sets the content presented by the section.</summary>
    public object? ChunkBody
    {
        get => GetValue(ChunkBodyProperty);
        set => SetValue(ChunkBodyProperty, value);
    }

    /// <inheritdoc />
    protected override IEnumerator LogicalChildren => EnumerateLogicalChildren();

    private static void OnChunkBodyChanged(
        DependencyObject dependencyObject,
        DependencyPropertyChangedEventArgs eventArgs
    )
    {
        var chunk = (Chunk)dependencyObject;
        if (eventArgs.OldValue is not null)
        {
            chunk.RemoveLogicalChild(eventArgs.OldValue);
        }

        if (eventArgs.NewValue is not null)
        {
            chunk.AddLogicalChild(eventArgs.NewValue);
        }
    }

    private IEnumerator EnumerateLogicalChildren()
    {
        if (ChunkBody is not null)
        {
            yield return ChunkBody;
        }
    }
}
