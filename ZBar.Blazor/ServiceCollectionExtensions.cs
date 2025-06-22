using Microsoft.Extensions.DependencyInjection;
using ZBar.Blazor.Interop;

namespace ZBar.Blazor
{
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Adds common services required by ZBar.Blazor components
        /// </summary>
        /// <param name="services">IServiceCollection</param>
        /// <returns>The continuing IServiceCollection chain</returns>
        public static IServiceCollection AddZBarServices(this IServiceCollection services)
        {
            return services.AddSingleton<CameraInterop>()
                .AddSingleton<ImageInterop>();
        }
    }
}