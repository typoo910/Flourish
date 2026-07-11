---
title: 提示浮层
description: 配置 Flourish 提示浮层的显示延迟。
---

# 提示浮层

提示浮层为图标按钮和紧凑命令提供补充说明。`UseTips` 设置显示延迟并立即启用提示浮层；Shell 会同时应用内置的窗口边界间距。

## 最小配置

```csharp
builder.ConfigureShell(shell => shell.UseTips(delay: 200));
```

延迟表示指针悬停后到提示出现前的时间，单位为毫秒。较短延迟会更快显示提示；较长延迟可减少指针经过控件时的意外显示。省略参数时使用 `200` 毫秒，延迟不能为负数。

提示浮层会使用默认边界间距，避免贴近 Shell 窗口边缘显示。该间距适用于标题栏、折叠导航栏、动态工具栏和状态栏附近的提示。

后台任务元信息提示不受 `UseTips` 控制。它属于[后台任务](background-tasks.md)状态界面，并使用自己的初始延迟；悬停等待队列图标时打开的是可交互浮层，而不是 tooltip。

## 相关功能

- [标题栏](configure-title-bar.md)、[导航](navigation.md)、[动态工具栏](dynamic-toolbar.md)和[状态栏](status-bar.md)包含受此设置控制的 Shell 提示。
- [后台任务](background-tasks.md)提供任务自身管理的元信息提示和可交互队列浮层。
- [动效](configure-motion.md)配置与提示浮层相互独立的过渡和悬停动画。
