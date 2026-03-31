using Jellyfin.Plugin.StudioCollections.Controllers;
using MediaBrowser.Common.Plugins;
using Microsoft.Extensions.DependencyInjection;

namespace Jellyfin.Plugin.StudioCollections
{
    /// <summary>
    /// Enregistre les services du plugin dans le conteneur DI de Jellyfin.
    /// </summary>
    public class PluginServiceRegistrator : IPluginServiceRegistrator
    {
        public void RegisterServices(IServiceCollection serviceCollection, IServiceProvider applicationServices)
        {
            // Le contrôleur est découvert automatiquement par ASP.NET.
            // Pas de services supplémentaires requis pour ce plugin.
        }
    }
}
