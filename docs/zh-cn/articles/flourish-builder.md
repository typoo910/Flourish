---
title: IFlourishBuilder
description: 理解 builder、Hosting 集成、服务注册和页面注册。
---

# IFlourishBuilder

`IFlourishBuilder` 是 Flourish 应用的组合入口。配置阶段不会立即创建窗口或页面；它会收集服务注册和 Shell 选项，最后在 `Build()` 时创建一个由 .NET Generic Host 支撑的 `IFlourish` 运行时。

## Hosting 模型

`FlourishBuilder.CreateDefaultBuilder(args)` 内部使用 `Host.CreateDefaultBuilder(args)`。因此构建出的运行时拥有现代 .NET 应用中熟悉的 Hosting 行为：

- 可以通过 `HostBuilderContext` 访问配置和环境信息
- 使用 `IServiceCollection` 注册服务
- 最终服务提供器可通过 `IFlourish.Services` 获取
- 应用对象可以用 `flourish.GetRequiredService<T>()` 解析
- `flourish.Start()` 和 `flourish.StopAsync()` 对应 Host 生命周期方法

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

## Builder 阶段

公开 builder 有四个配置阶段。

| 方法 | 作用 |
| --- | --- |
| `ConfigureServices` | 注册应用服务、页面、命令解析器、ViewModel 以及你希望放入 DI 的基础设施。 |
| `ConfigureShell` | 配置 Shell 窗口、标题栏、导航栏、动效、材质特效、字体和动态工具栏区域。 |
| `ConfigureDynamicToolbar` | 注册按页面变化的工具栏项。 |
| `ConfigureStatus` | 配置 Shell 底部状态栏。 |

这些方法都可以调用多次。Flourish 会保存回调，并在 `Build()` 时统一应用。

## 注册服务

所有属于依赖注入的内容都放在 `ConfigureServices` 中。

```csharp
builder.ConfigureServices((_, services) =>
{
    services.AddSingleton<App>();
    services.AddSingleton<ICommandParser, AppCommandParser>();
    services.AddSingleton<ImageLibrary>();
    services.AddTransient<EditorViewModel>();
});
```

Flourish 会在构建阶段注册自己的内部服务，包括导航、工具栏、状态栏、材质特效、动效、页面缓存和 Shell 窗口服务。你不需要直接构造这些内部服务。

## 使用 AddNavigable 注册页面

`AddNavigable` 是让 WPF `Page` 进入 Shell 导航模型的推荐方式。它会同时把页面注册到 DI，并记录 Flourish 用于导航 UI 的显示元数据。

```csharp
services.AddNavigable<HomePage>(
    displayName: "首页",
    iconGlyph: "\uE80F",
    isInitial: true,
    cacheMode: FlourishPageCacheMode.Enabled);
```

泛型重载适合页面类型在编译期已知的场景。

```csharp
services.AddNavigable<SettingsPage>("设置", "\uE713");
```

`Type` 重载适合页面注册来自配置或插件的场景。

```csharp
services.AddNavigable(
    typeof(ReportPage),
    displayName: "报表",
    iconGlyph: "\uE9D2",
    cacheMode: FlourishPageCacheMode.Disabled);
```

`displayName` 会显示在导航 UI 中。`iconGlyph` 通常是 Segoe Fluent Icons 字形，例如 `"\uE80F"`。`isInitial` 指定 Shell 打开后首先导航到的页面。`cacheMode` 控制页面实例是否复用。

## 页面缓存模式

如果页面需要保留局部 UI 状态、滚动位置或已加载数据，使用 `FlourishPageCacheMode.Enabled`。如果页面每次进入都应重新创建，例如总是要加载新状态的向导页或编辑页，使用 `Disabled`。

```csharp
services.AddNavigable<DashboardPage>(
    "仪表盘",
    "\uE9D2",
    cacheMode: FlourishPageCacheMode.Enabled);

services.AddNavigable<ImportWizardPage>(
    "导入",
    "\uE8B5",
    cacheMode: FlourishPageCacheMode.Disabled);
```

## 构建运行时

`Build()` 会创建 Host 并返回 `IFlourish`。调用之后，配置阶段就结束了。

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

只要 WPF 应用还在运行，就应保持这个运行时实例存活。
