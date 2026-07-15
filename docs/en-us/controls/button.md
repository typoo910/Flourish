---
title: Button
description: Use Flourish Button, IconButton, CardButton, and WindowCaptionButton to communicate action semantics.
---

# Button

The Flourish button family consists of one general-purpose text button and three specialized derivatives. They keep the command, click event, content, keyboard, and automation behavior of WPF `Button` while supplying Flourish theme, focus, and pointer feedback.

| Control | Use for |
| --- | --- |
| `Button` | Ordinary actions expressed with text. |
| `IconButton` | Icon-only actions or actions that need an icon before their text. |
| `CardButton` | Navigation or selection actions where the complete card is invokable. |
| `WindowCaptionButton` | Minimize, maximize, restore, and close actions in a window caption. |

## Button

`Button.Appearance` communicates semantic emphasis, not button size or layout. Its default is `Standard`.

| `ButtonAppearance` | When to use it |
| --- | --- |
| `Standard` | A default action or an ordinary option alongside other actions. |
| `Primary` | The single most important action in a group. |
| `Subtle` | A supporting or toolbar-like action that needs less visual emphasis. |
| `Danger` | A destructive or difficult-to-reverse action such as delete or reset. |

```xml
<StackPanel Orientation="Horizontal">
  <flourish:Button
    Appearance="Primary"
    Command="{Binding SaveCommand}"
    Content="Save" />
  <flourish:Button
    Command="{Binding CancelCommand}"
    Content="Cancel" />
  <flourish:Button
    Appearance="Danger"
    Command="{Binding DeleteCommand}"
    Content="Delete" />
</StackPanel>
```

An action group should normally have only one `Primary` button. Let the containing layout control external placement. Do not use `Appearance` to select structural dimensions.

## IconButton

`IconButton` adds an `Icon` property of type `object?` with a default of `null`. It accepts a glyph string from the Flourish icon font or any visual element. Inherited `Content` is optional and becomes the text label after the icon.

```xml
<flourish:IconButton
  Appearance="Subtle"
  AutomationProperties.Name="Refresh"
  Command="{Binding RefreshCommand}"
  Icon="&#xE72C;"
  ToolTip="Refresh" />

<flourish:IconButton
  Appearance="Primary"
  Command="{Binding AddCommand}"
  Content="Add item"
  Icon="&#xE710;" />
```

When `Content` is `null`, `IconButton` uses compact `30 × 30` geometry without padding. Give every icon-only button a visible `ToolTip` and an `AutomationProperties.Name` that identifies the action. Simple tooltip content such as a string is automatically wrapped in a `FlourishToolTip` so it follows the Shell-aware placement rules.

## CardButton

`CardButton` represents an interactive card rather than an appearance variant of an ordinary button. Use it when invoking the complete card is the action. If only a button within the card is interactive, use a non-interactive card container with an ordinary `Button` instead.

| Property | Type | Default | Purpose |
| --- | --- | --- | --- |
| `Title` | `string` | `""` | The card heading. |
| `Content` | inherited `object?` | `null` | The card description or other supporting content. |
| `Icon` | `object?` | `null` | A card icon or other visual content. |
| `IconPosition` | `Dock` | `Top` | Places the icon at `Left`, `Top`, `Right`, or `Bottom`. |

```xml
<flourish:CardButton
  Command="{Binding OpenReportsCommand}"
  Content="Review generated reports and recent exports."
  Icon="&#xE8A5;"
  IconPosition="Left"
  Title="Reports" />
```

## WindowCaptionButton

`WindowCaptionButton` is reserved for window captions. It inherits `Icon`, `Appearance`, and the standard WPF button contract from `IconButton`, but uses dedicated caption geometry with a default size of `46 × 40`.

Set a close action to `Appearance="Danger"` and use `Subtle` for the other caption actions. The control supplies presentation and button interaction only; connect the actual window operation through `Command` or `Click`.

```xml
<flourish:WindowCaptionButton
  Appearance="Danger"
  AutomationProperties.Name="Close"
  Command="{Binding CloseWindowCommand}"
  Icon="&#xE8BB;"
  ToolTip="Close" />
```

## Hover feedback and reduced motion

The button family participates in the public `HoverReveal` attached behavior. Prefer application-wide configuration through [Motion](../articles/configure-motion.md), including the operating system reduced-motion preference. Set the attached properties directly only for a local override:

```xml
<flourish:Button
  flourish:HoverReveal.AnimationDuration="0:0:0.14"
  flourish:HoverReveal.IsEnabled="True"
  flourish:HoverReveal.OverrideColor="{DynamicResource FlourishPrimarySurfaceBrush}"
  Content="Preview" />
```

`HoverReveal.IsEnabled` defaults to `true` and inherits, `AnimationDuration` defaults to 140 milliseconds and inherits, and `OverrideColor` defaults to `null` and does not inherit. A `Danger` button supplies a danger-semantic hover color by default; a local `OverrideColor` takes precedence.

## Related content

- [Chunk](chunk.md) explains how to organize buttons and card content within page sections.
- [Motion](../articles/configure-motion.md) configures hover reveal and reduced motion.
- The [Button API](xref:ArkheideSystem.Flourish.Controls.Button), [IconButton API](xref:ArkheideSystem.Flourish.Controls.IconButton), [CardButton API](xref:ArkheideSystem.Flourish.Controls.CardButton), and [WindowCaptionButton API](xref:ArkheideSystem.Flourish.Controls.WindowCaptionButton) list all members.
