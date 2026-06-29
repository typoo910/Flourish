using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace AcksheedSys.Flourish.Abstract;

public interface IFlourishBuilder
{
    IFlourishBuilder ConfigureServices(Action<HostBuilderContext, IServiceCollection> configureServices);

    IFlourishBuilder ConfigureShell(Action<HostBuilderContext, IFlourishShellBuilder> configureShell);

    IFlourishBuilder ConfigureDynamicToolbar(Action<HostBuilderContext, IFlourishDynamicToolbarBuilder> configureToolbar);

    IFlourishBuilder ConfigureStatus(Action<HostBuilderContext, IFlourishStatusBuilder> configureStatus);

    IFlourish Build();
}
