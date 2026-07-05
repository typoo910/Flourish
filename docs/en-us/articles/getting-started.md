---
title: Getting started
description: Apply Flourish to a WPF application with the shortest useful path.
---

# Getting started

The fastest way to use Flourish is to let the shell host your WPF `Application`: add the theme resources, build an `IFlourish` runtime in `Program.Main`, register pages with `AddNavigable`, place them in the navigation panel with `UseNavigationPanel`, then call `Program.Flourish.Show(this)` when the application starts.

## Reference the theme

`IFlourish.Show(Application)` automatically merges `/Flourish;component/Themes/Generic.xaml` into the application resources before it opens the shell. You can still add it explicitly in `App.xaml`; this is useful for the WPF designer and for resources used before the shell is shown.

```xml
<Application
  x:Class="MyApp.App"
  xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
  xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml">
  <Application.Resources>
    <ResourceDictionary>
      <ResourceDictionary.MergedDictionaries>
        <ResourceDictionary Source="/Flourish;component/Themes/Generic.xaml" />
      </ResourceDictionary.MergedDictionaries>
    </ResourceDictionary>
  </Application.Resources>
</Application>
```

## Create a Program entry point

Store the built runtime in a static property so `App` and pages can reach services consistently.

```csharp
using AcksheedSys.Flourish.Abstract;
using Microsoft.Extensions.DependencyInjection;

namespace MyApp;

internal static class Program
{
    private static IFlourish? flourish;

    public static IFlourish Flourish =>
        flourish ?? throw new InvalidOperationException("Flourish has not been built.");

    [STAThread]
    public static int Main(string[] args)
    {
        flourish = FlourishBuilder
            .CreateDefaultBuilder(args)
            .ConfigureServices((_, services) =>
            {
                services.AddSingleton<App>();
                services.AddSingleton<ICommandParser, AppCommandParser>();

                services.AddNavigable<HomePage>("Home", "\uE80F");
                services.AddNavigable<SettingsPage>("Settings", "\uE713");
            })
            .ConfigureShell((_, shell) =>
            {
                shell
                    .UseTitlebar((_, titlebar) =>
                    {
                        titlebar
                            .ShowLogo()
                            .ShowTitle()
                            .ShowSearch()
                            .ShowBreadcrumb()
                            .ShowNavToggle()
                            .SetTitle("My App")
                            .SetSubtitle("Flourish shell");
                    })
                    .UseNavigationPanel((_, nav) =>
                    {
                        nav.SetInitiallyOpen()
                           .SetGroup("Navigation", groupId: 0, group =>
                           {
                               group.AddNavigableViewItem<HomePage>(isInitial: true);
                           })
                           .AddFixedNavigableViewItem<SettingsPage>();
                    })
                    .UseDynamicToolbar()
                    .UseMotion()
                    .UseMaterialEffect()
                    .SetGlobalFont("Microsoft YaHei")
                    .SetWindowProperty((_, window) =>
                    {
                        window.SetWindowSize(1280, 720).SetWindowMinSize(960, 540);
                    });
            })
            .Build();

        try
        {
            flourish.Start();
            var app = flourish.GetRequiredService<App>();
            return app.Run();
        }
        finally
        {
            flourish.StopAsync().GetAwaiter().GetResult();
            flourish.Dispose();
            flourish = null;
        }
    }
}
```

`CreateDefaultBuilder(args)` creates a standard .NET Generic Host. That means `ConfigureServices` receives a normal `IServiceCollection`, so your application services, pages, and command parsers all use familiar dependency injection patterns.

## Show the Flourish shell

In `App.xaml.cs`, call `Program.Flourish.Show(this)` from startup. This creates the `FlourishShellWindow`, assigns it as `Application.MainWindow`, merges the theme if needed, and opens the shell.

```csharp
using System.Windows;

namespace MyApp;

public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        Program.Flourish.Show(this);
    }
}
```

## Create pages

Pages registered with `AddNavigable` are regular WPF `Page` classes. Flourish resolves them from dependency injection when navigation occurs. The page display name, icon, and cache mode come from `AddNavigable`; the visible navigation position and initial page are configured in `UseNavigationPanel`.

```csharp
using System.Windows.Controls;

namespace MyApp;

public partial class HomePage : Page
{
    public HomePage()
    {
        InitializeComponent();
    }
}
```

## First run checklist

- `App` is registered with `services.AddSingleton<App>()`.
- At least one page is registered with `AddNavigable`.
- At least one visible page item is added with `AddNavigableViewItem`, preferably with `isInitial: true`.
- `App.OnStartup` calls `Program.Flourish.Show(this)`.
- `flourish.Start()` is called before `app.Run()`.
- `StopAsync()` and `Dispose()` are called in `finally`.
