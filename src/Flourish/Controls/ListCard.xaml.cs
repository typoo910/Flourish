using System.Collections;
using System.Windows;
using WpfHorizontalAlignment = System.Windows.HorizontalAlignment;
using WpfVerticalAlignment = System.Windows.VerticalAlignment;

namespace ArkheideSystem.Flourish.Controls;

/// <summary>
/// A compact, non-interactive configuration surface with a left presenter, centered copy,
/// and a right-aligned body. The surface always uses the Standard card variant, while
/// interactive controls may be placed inside <see cref="Card.Body" />.
/// </summary>
/// <remarks>
/// <see cref="Presenter" />, the inherited copy, and <see cref="Card.Body" /> are vertically
/// centered in one row. The inherited variant and content-alignment properties are coerced
/// to <see cref="Variant.Standard" />, horizontal stretch, and vertical center. Title and
/// supporting text are rendered as single trimmed lines. Body should contain one local control;
/// selection and toggle controls should normally apply their value immediately.
/// </remarks>
public class ListCard : Card
{
    /// <summary>Identifies the <see cref="Presenter" /> dependency property.</summary>
    public static readonly DependencyProperty PresenterProperty = DependencyProperty.Register(
        nameof(Presenter),
        typeof(object),
        typeof(ListCard),
        new FrameworkPropertyMetadata(null, OnPresenterChanged)
    );

    static ListCard()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(ListCard),
            new FrameworkPropertyMetadata(typeof(ListCard))
        );
        VariantProperty.OverrideMetadata(
            typeof(ListCard),
            new FrameworkPropertyMetadata(Variant.Standard, null, CoerceVariant)
        );
        ContentHorizontalAlignmentProperty.OverrideMetadata(
            typeof(ListCard),
            new FrameworkPropertyMetadata(
                WpfHorizontalAlignment.Stretch,
                null,
                CoerceHorizontalAlignment
            )
        );
        ContentVerticalAlignmentProperty.OverrideMetadata(
            typeof(ListCard),
            new FrameworkPropertyMetadata(
                WpfVerticalAlignment.Center,
                null,
                CoerceVerticalAlignment
            )
        );
    }

    /// <summary>
    /// Gets or sets the icon, image, or custom visual displayed in the fixed left region.
    /// </summary>
    public object? Presenter
    {
        get => GetValue(PresenterProperty);
        set => SetValue(PresenterProperty, value);
    }

    /// <inheritdoc />
    protected override IEnumerator LogicalChildren => EnumerateLogicalChildren();

    private static object CoerceVariant(DependencyObject dependencyObject, object value)
    {
        return Variant.Standard;
    }

    private static object CoerceHorizontalAlignment(
        DependencyObject dependencyObject,
        object value
    )
    {
        return WpfHorizontalAlignment.Stretch;
    }

    private static object CoerceVerticalAlignment(
        DependencyObject dependencyObject,
        object value
    )
    {
        return WpfVerticalAlignment.Center;
    }

    private static void OnPresenterChanged(
        DependencyObject dependencyObject,
        DependencyPropertyChangedEventArgs eventArgs
    )
    {
        var card = (ListCard)dependencyObject;
        if (eventArgs.OldValue is not null)
        {
            card.RemoveLogicalChild(eventArgs.OldValue);
        }

        if (eventArgs.NewValue is not null)
        {
            card.AddLogicalChild(eventArgs.NewValue);
        }
    }

    private IEnumerator EnumerateLogicalChildren()
    {
        var baseChildren = base.LogicalChildren;
        while (baseChildren.MoveNext())
        {
            yield return baseChildren.Current;
        }

        if (Presenter is not null)
        {
            yield return Presenter;
        }
    }
}
