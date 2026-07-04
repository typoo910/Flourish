---
title: IFlourishBuilder
description: Understand the builder, hosting integration, services, and page registration.
---

# IFlourishBuilder

`IFlourishBuilder` is the composition entry point for a Flourish application. It does not directly create windows or pages while you configure it. Instead, it collects service registrations and shell options, then builds an `IFlourish` runtime backed by a .NET Generic Host.

## Hosting model

`FlourishBuilder.CreateDefaultBuilder(args)` uses `Host.CreateDefaultBuilder(args)` internally. The resulting runtime therefore has the same basic hosting behavior you expect from modern .NET applications:

- configuration and environment are available through `HostBuilderContext`
- services are registered in `IServiceCollection`
- the final service provider is available from `IFlourish.Services`
- application objects can be resolved with `flourish.GetRequiredService<T>()`
- `flourish.Start()` and `flourish.StopAsync()` map to host lifecycle methods

```csharp
var flourish = FlourishBuilder
    .CreateDefaultBuilder(args)
    .ConfigureServices((context, services) =>
    {
        services.AddSingleton<App>();
        services.AddSingleton<ICommandParser, AppCommandParser>();
    })
    .Build();

flourish.Start();
var app = flourish.GetRequiredService<App>();
```

## Builder stages

The public builder has four configuration stages.

| Method | Purpose |
| --- | --- |
| `ConfigureServices` | Registers application services, pages, command parsers, view models, and any infrastructure you want in DI. |
| `ConfigureShell` | Configures the shell window, title bar, navigation panel, motion, material effect, font, and dynamic toolbar surface. |
| `ConfigureDynamicToolbar` | Registers page-specific toolbar items. |
| `ConfigureStatus` | Configures the status area at the bottom of the shell. |

Each method can be called multiple times. Flourish stores the callbacks and applies them during `Build()`.

## Register services

Use `ConfigureServices` for anything that belongs to dependency injection.

```csharp
builder.ConfigureServices((_, services) =>
{
    services.AddSingleton<App>();
    services.AddSingleton<ICommandParser, AppCommandParser>();
    services.AddSingleton<ImageLibrary>();
    services.AddTransient<EditorViewModel>();
});
```

Flourish also registers its own internal services during build, including navigation, toolbar, status, material effect, motion, page cache, and shell window services. You do not need to construct those directly.

## Register pages with AddNavigable

`AddNavigable` is the recommended way to make a WPF `Page` appear in the shell navigation model. It both registers the page in DI and records the display metadata used by Flourish.

```csharp
services.AddNavigable<HomePage>(
    displayName: "Home",
    iconGlyph: "\uE80F",
    isInitial: true,
    cacheMode: FlourishPageCacheMode.Enabled);
```

The generic overload is best when the page type is known at compile time.

```csharp
services.AddNavigable<SettingsPage>("Settings", "\uE713");
```

The `Type` overload is useful when page registrations come from configuration or plugins.

```csharp
services.AddNavigable(
    typeof(ReportPage),
    displayName: "Reports",
    iconGlyph: "\uE9D2",
    cacheMode: FlourishPageCacheMode.Disabled);
```

`displayName` is shown in navigation UI. `iconGlyph` is typically a Segoe Fluent Icons glyph such as `"\uE80F"`. `isInitial` marks the first page Flourish should navigate to when the shell opens. `cacheMode` controls whether the same page instance is reused.

## Page cache mode

Use `FlourishPageCacheMode.Enabled` for pages that should keep local UI state, scroll position, or loaded data while the user navigates away and back. Use `Disabled` for pages that should be recreated every time, such as editors that must always load fresh state.

```csharp
services.AddNavigable<DashboardPage>(
    "Dashboard",
    "\uE9D2",
    cacheMode: FlourishPageCacheMode.Enabled);

services.AddNavigable<ImportWizardPage>(
    "Import",
    "\uE8B5",
    cacheMode: FlourishPageCacheMode.Disabled);
```

## Build the runtime

`Build()` creates the host and returns `IFlourish`. After that, configuration is complete.

```csharp
using var flourish = builder.Build();
flourish.Start();

try
{
    var app = flourish.GetRequiredService<App>();
    app.Run();
}
finally
{
    await flourish.StopAsync();
}
```

Keep the runtime alive for as long as the WPF application is running.
