---
title: Button
description: 使用 Flourish Button、IconButton、CardButton 与 WindowCaptionButton 表达操作语义。
---

# Button

Flourish 按钮家族由一个通用文本按钮和三个专用衍生控件组成。它们保留 WPF `Button` 的命令、点击事件、内容、键盘和自动化行为，并提供 Flourish 的主题、焦点与指针反馈。

| 控件 | 用于 |
| --- | --- |
| `Button` | 以文字表达的常规操作。 |
| `IconButton` | 仅图标操作，或需要在文字前显示图标的操作。 |
| `CardButton` | 整个卡片都可调用的导航或选择操作。 |
| `WindowCaptionButton` | 窗口标题栏中的最小化、最大化、还原与关闭操作。 |

## Button

`Button.Appearance` 表达操作的语义强调，而不是按钮的尺寸或布局。默认值是 `Standard`。

| `ButtonAppearance` | 使用时机 |
| --- | --- |
| `Standard` | 默认操作或与其他操作并列的普通选项。 |
| `Primary` | 一组操作中最主要的一个。 |
| `Subtle` | 辅助性、工具栏式或需要降低视觉强调的操作。 |
| `Danger` | 删除、重置等破坏性或难以撤销的操作。 |

```xml
<StackPanel Orientation="Horizontal">
  <flourish:Button
    Appearance="Primary"
    Command="{Binding SaveCommand}"
    Content="保存" />
  <flourish:Button
    Command="{Binding CancelCommand}"
    Content="取消" />
  <flourish:Button
    Appearance="Danger"
    Command="{Binding DeleteCommand}"
    Content="删除" />
</StackPanel>
```

一组操作通常只应有一个 `Primary` 按钮。外部位置由包含按钮的布局容器控制；不要通过 `Appearance` 选择结构尺寸。

## IconButton

`IconButton` 增加了类型为 `object?`、默认值为 `null` 的 `Icon` 属性。它可以接受 Flourish 图标字体的字形字符串，也可以接受任意可视元素。继承的 `Content` 可选：设置后它会成为图标后的文字标签。

```xml
<flourish:IconButton
  Appearance="Subtle"
  AutomationProperties.Name="刷新"
  Command="{Binding RefreshCommand}"
  Icon="&#xE72C;"
  ToolTip="刷新" />

<flourish:IconButton
  Appearance="Primary"
  Command="{Binding AddCommand}"
  Content="添加项目"
  Icon="&#xE710;" />
```

当 `Content` 为 `null` 时，`IconButton` 使用无内边距的紧凑 `30 × 30` 布局。仅图标按钮应同时设置可见的 `ToolTip` 和可识别操作意图的 `AutomationProperties.Name`。字符串等简单 `ToolTip` 内容会自动包装为 `FlourishToolTip`，从而遵循 Shell 感知的定位规则。

## CardButton

`CardButton` 表示一块可交互的卡片，而不是普通按钮的外观变体。它应用于调用整个卡片的操作；如果只有卡片内的某个按钮可交互，请使用非交互卡片容器和普通 `Button`。

| 属性 | 类型 | 默认值 | 用途 |
| --- | --- | --- | --- |
| `Title` | `string` | `""` | 卡片标题。 |
| `Content` | 继承的 `object?` | `null` | 卡片的描述或其他辅助内容。 |
| `Icon` | `object?` | `null` | 卡片图标或其他可视内容。 |
| `IconPosition` | `Dock` | `Top` | 将图标放在 `Left`、`Top`、`Right` 或 `Bottom`。 |

```xml
<flourish:CardButton
  Command="{Binding OpenReportsCommand}"
  Content="查看已生成的报告与最近导出。"
  Icon="&#xE8A5;"
  IconPosition="Left"
  Title="报告" />
```

## WindowCaptionButton

`WindowCaptionButton` 只用于窗口标题栏。它继承 `IconButton` 的 `Icon`、`Appearance` 和标准 WPF 按钮契约，但使用标题栏专用几何，默认尺寸为 `46 × 40`。

将关闭操作设置为 `Appearance="Danger"`，其他标题栏操作使用 `Subtle`。该控件只负责显示与按钮交互；应用仍需要通过 `Command` 或 `Click` 连接实际的窗口操作。

```xml
<flourish:WindowCaptionButton
  Appearance="Danger"
  AutomationProperties.Name="关闭"
  Command="{Binding CloseWindowCommand}"
  Icon="&#xE8BB;"
  ToolTip="关闭" />
```

## 悬停反馈与减少动态效果

按钮家族参与公共 `HoverReveal` 附加行为。优先在应用级别通过[动效](../articles/configure-motion.md)配置它，并遵循操作系统的减少动态效果偏好。需要局部覆盖时，可以设置以下附加属性：

```xml
<flourish:Button
  flourish:HoverReveal.AnimationDuration="0:0:0.14"
  flourish:HoverReveal.IsEnabled="True"
  flourish:HoverReveal.OverrideColor="{DynamicResource FlourishPrimarySurfaceBrush}"
  Content="预览" />
```

`HoverReveal.IsEnabled` 默认为 `true` 并向下继承，`AnimationDuration` 默认为 140 毫秒并向下继承，`OverrideColor` 默认为 `null` 且不继承。`Danger` 按钮默认使用危险语义的悬停颜色；局部 `OverrideColor` 会覆盖它。

## 相关内容

- [Chunk](chunk.md)说明如何在页面章节中组织按钮与卡片内容。
- [动效](../articles/configure-motion.md)配置悬停显示与减少动态效果。
- [Button API](xref:ArkheideSystem.Flourish.Controls.Button)、[IconButton API](xref:ArkheideSystem.Flourish.Controls.IconButton)、[CardButton API](xref:ArkheideSystem.Flourish.Controls.CardButton) 与 [WindowCaptionButton API](xref:ArkheideSystem.Flourish.Controls.WindowCaptionButton) 列出完整成员。
