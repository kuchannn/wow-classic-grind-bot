using Microsoft.Extensions.DependencyInjection;

namespace Frontend;

public static class DependencyInjection
{
    public static IServiceCollection AddFrontend(this IServiceCollection services)
    {
        services.AddBlazorBootstrap();

        services.AddRazorPages();

        services.AddRazorComponents()
            .AddInteractiveServerComponents();

        return services;
    }
}
