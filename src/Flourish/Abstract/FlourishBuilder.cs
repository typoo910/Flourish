using Flourish.Internal;
using Flourish.Models;
using Flourish.Services;
using Flourish.Windows;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace AcksheedSys.Flourish.Abstract;

public static class FlourishBuilder
{
    public static IFlourishBuilder CreateDefaultBuilder(string[] args)
    {
        return new DefaultFlourishBuilder(args);
    }
}

internal sealed class DefaultFlourishBuilder : IFlourishBuilder
{
    private readonly IHostBuilder hostBuilder;
    private readonly FlourishShellOptions shellOptions = new();
    private readonly List<Action<HostBuilderContext, IServiceCollection>> serviceConfigurations = [];
    private readonly List<Action<HostBuilderContext, IFlourishShellBuilder>> shellConfigurations = [];
    private readonly List<Action<HostBuilderContext, IFlourishDynamicToolbarBuilder>> toolbarConfigurations = [];
    private readonly List<Action<HostBuilderContext, IFlourishStatusBuilder>> statusConfigurations = [];

    public DefaultFlourishBuilder(string[] args)
    {
        hostBuilder = Host.CreateDefaultBuilder(args);
    }

    public IFlourishBuilder ConfigureServices(
        Action<HostBuilderContext, IServiceCollection> configureServices
    )
    {
        serviceConfigurations.Add(configureServices);
        return this;
    }

    public IFlourishBuilder ConfigureShell(
        Action<HostBuilderContext, IFlourishShellBuilder> configureShell
    )
    {
        shellConfigurations.Add(configureShell);
        return this;
    }

    public IFlourishBuilder ConfigureDynamicToolbar(
        Action<HostBuilderContext, IFlourishDynamicToolbarBuilder> configureToolbar
    )
    {
        toolbarConfigurations.Add(configureToolbar);
        return this;
    }

    public IFlourishBuilder ConfigureStatus(
        Action<HostBuilderContext, IFlourishStatusBuilder> configureStatus
    )
    {
        statusConfigurations.Add(configureStatus);
        return this;
    }

    public IFlourish Build()
    {
        hostBuilder.ConfigureServices(
            (context, services) =>
            {
                foreach (var configureServices in serviceConfigurations)
                {
                    configureServices(context, services);
                }

                ApplyFlourishConfigurations(context);
                ApplyServiceCollectionRegistrations(services);

                services.AddSingleton(shellOptions);
                services.AddSingleton<FlourishShellWindow>();
                services.AddSingleton<PageHistoryService>();
                services.AddSingleton<global::Flourish.Services.INavigationService, NavigationService>();
            }
        );

        return new FlourishRuntime(hostBuilder.Build());
    }

    private void ApplyFlourishConfigurations(HostBuilderContext context)
    {
        var shellBuilder = new FlourishShellBuilder(shellOptions);
        foreach (var configureShell in shellConfigurations)
        {
            configureShell(context, shellBuilder);
        }

        var toolbarBuilder = new FlourishDynamicToolbarBuilder(shellOptions);
        foreach (var configureToolbar in toolbarConfigurations)
        {
            configureToolbar(context, toolbarBuilder);
        }

        var statusBuilder = new FlourishStatusBuilder(shellOptions);
        foreach (var configureStatus in statusConfigurations)
        {
            configureStatus(context, statusBuilder);
        }
    }

    private void ApplyServiceCollectionRegistrations(IServiceCollection services)
    {
        var state = services
            .FirstOrDefault(descriptor =>
                descriptor.ServiceType == typeof(FlourishServiceCollectionState)
                && descriptor.ImplementationInstance is FlourishServiceCollectionState
            )
            ?.ImplementationInstance as FlourishServiceCollectionState;

        if (state is null)
        {
            return;
        }

        foreach (var page in state.NavigablePages)
        {
            var key = page.PageType.FullName ?? page.DisplayName;
            if (shellOptions.NavigationItems.Any(item => item.Key == key))
            {
                continue;
            }

            shellOptions.NavigationItems.Add(
                new FlourishNavigationItem(key, page.DisplayName, page.IconGlyph, page.PageType)
            );

            if (page.IsInitial)
            {
                shellOptions.InitialNavigationPageType = page.PageType;
            }
        }
    }
}
