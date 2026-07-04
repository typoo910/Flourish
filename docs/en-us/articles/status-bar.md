---
title: Status bar
description: Configure the Flourish shell status area.
---

# Status bar

The status bar is configured through `ConfigureStatus`. It is intended for low-priority state such as readiness, connection state, power state, or short contextual messages.

```csharp
builder.ConfigureStatus((_, status) =>
{
    status
        .SetStatusText("Ready")
        .AddStatusItem("Online", "\uE774")
        .ShowLANConnectionStatus()
        .ShowPowerStatus();
});
```

## Primary status text

`SetStatusText` sets the main text in the status area.

```csharp
status.SetStatusText("Ready");
```

Use it for stable state, not for long logs or notifications. Keep the text short so it remains readable in smaller windows.

## Custom status items

`AddStatusItem` adds a compact item with display text and a glyph.

```csharp
status.AddStatusItem("Online", "\uE774");
status.AddStatusItem("Synced", "\uE73E");
```

Use custom status items for application-specific state such as account state, workspace name, sync state, or current mode.

## Built-in status items

`ShowLANConnectionStatus` adds the built-in LAN connection indicator. `ShowPowerStatus` adds the built-in power indicator.

```csharp
status.ShowLANConnectionStatus();
status.ShowPowerStatus();
```

These are useful for desktop tools where network or battery state affects the user workflow.

## Where to configure it

Status configuration belongs beside the rest of application composition.

```csharp
var flourish = FlourishBuilder
    .CreateDefaultBuilder(args)
    .ConfigureStatus((_, status) =>
    {
        status.SetStatusText("Ready").ShowLANConnectionStatus().ShowPowerStatus();
    })
    .Build();
```

The status bar is part of the shell, so configure it once at startup.
