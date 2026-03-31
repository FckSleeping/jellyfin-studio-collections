using System;
using MediaBrowser.Common.Configuration;
using MediaBrowser.Common.Plugins;
using MediaBrowser.Model.Serialization;
using Jellyfin.Plugin.StudioCollections.Configuration;

namespace Jellyfin.Plugin.StudioCollections
{
    public class Plugin : BasePlugin<PluginConfiguration>
    {
        public Plugin(IApplicationPaths applicationPaths, IXmlSerializer xmlSerializer)
            : base(applicationPaths, xmlSerializer)
        {
            Instance = this;
        }

        public static Plugin? Instance { get; private set; }

        public override string Name => "StudioCollections";

        public override Guid Id => Guid.Parse("a3f2c1d4-5e6b-7890-abcd-ef1234567890");

        public override string Description => "Affiche une section par studio avec logos et effet hover (intro/backdrop) dans l'interface Jellyfin.";
    }
}
