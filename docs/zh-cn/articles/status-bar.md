---
title: 状态栏
description: 配置 Flourish Shell 状态区域。
---

# 状态栏

状态栏通过 `ConfigureStatus` 配置，适合显示低优先级状态，例如就绪状态、连接状态、电源状态或简短上下文信息。

```csharp
builder.ConfigureStatus((_, status) =>
{
    status
        .SetStatusText("就绪")
        .AddStatusItem("在线", "\uE774")
        .ShowLANConnectionStatus()
        .ShowPowerStatus();
});
```

## 主状态文本

`SetStatusText` 设置状态区域中的主文本。

```csharp
status.SetStatusText("就绪");
```

它适合稳定状态，不适合长日志或通知。文本应保持简短，确保小窗口下也能阅读。

## 自定义状态项

`AddStatusItem` 添加一个带显示文本和字形的紧凑状态项。

```csharp
status.AddStatusItem("在线", "\uE774");
status.AddStatusItem("已同步", "\uE73E");
```

自定义状态项适合显示应用专属状态，例如账号状态、工作区名称、同步状态或当前模式。

## 内置状态项

`ShowLANConnectionStatus` 添加内置局域网连接状态。`ShowPowerStatus` 添加内置电源状态。

```csharp
status.ShowLANConnectionStatus();
status.ShowPowerStatus();
```

如果网络或电池状态会影响用户工作流，这些内置项会比较有用。

## 放在哪里配置

状态栏配置应和其他应用组合配置放在一起。

```csharp
var flourish = FlourishBuilder
    .CreateDefaultBuilder(args)
    .ConfigureStatus((_, status) =>
    {
        status.SetStatusText("就绪").ShowLANConnectionStatus().ShowPowerStatus();
    })
    .Build();
```

状态栏是 Shell 的一部分，通常在启动阶段配置一次即可。
