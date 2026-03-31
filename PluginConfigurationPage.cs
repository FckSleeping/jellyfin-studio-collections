using System.IO;
using System.Reflection;
using MediaBrowser.Common.Plugins;

namespace Jellyfin.Plugin.StudioCollections.Configuration
{
    /// <summary>
    /// Enregistre la page de configuration dans le dashboard Jellyfin.
    /// </summary>
    public class PluginConfigurationPage : IPluginConfigurationPage
    {
        public string Name => "StudioCollections";

        public ConfigurationPageType ConfigurationPageType => ConfigurationPageType.PluginConfiguration;

        public IPlugin Plugin => StudioCollections.Plugin.Instance!;

        public Stream GetHtml()
        {
            return Assembly.GetExecutingAssembly()
                .GetManifestResourceStream("Jellyfin.Plugin.StudioCollections.Configuration.configPage.html")!;
        }
    }
}
