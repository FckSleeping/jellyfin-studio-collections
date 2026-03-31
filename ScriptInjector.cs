using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MediaBrowser.Controller.Plugins;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Plugin.StudioCollections
{
    /// <summary>
    /// Point d'entrée serveur : injecte le script JS dans le fichier index.html de Jellyfin Web.
    /// </summary>
    public class ScriptInjector : IServerEntryPoint
    {
        private readonly ILogger<ScriptInjector> _logger;
        private const string ScriptTag = "<!-- StudioCollections:injected -->";
        private const string ScriptSrc = "<script src=\"/StudioCollections/clientscript\" defer></script>";

        public ScriptInjector(ILogger<ScriptInjector> logger)
        {
            _logger = logger;
        }

        public Task RunAsync()
        {
            InjectScript();
            return Task.CompletedTask;
        }

        private void InjectScript()
        {
            try
            {
                // Chemins courants de index.html selon la distrib Jellyfin
                var candidates = new[]
                {
                    "/usr/share/jellyfin/web/index.html",
                    "/jellyfin/jellyfin-web/index.html",
                    Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "jellyfin-web", "index.html"),
                    Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "jellyfin-web", "index.html"),
                };

                foreach (var path in candidates)
                {
                    if (!File.Exists(path)) continue;

                    var content = File.ReadAllText(path, Encoding.UTF8);

                    // Déjà injecté ?
                    if (content.Contains(ScriptTag)) return;

                    // Injecter avant </body>
                    var injection = $"\n{ScriptTag}\n{ScriptSrc}\n</body>";
                    content = content.Replace("</body>", injection, StringComparison.OrdinalIgnoreCase);

                    File.WriteAllText(path, content, Encoding.UTF8);
                    _logger.LogInformation("[StudioCollections] Script injecté dans {Path}", path);
                    return;
                }

                _logger.LogWarning("[StudioCollections] index.html introuvable, injection impossible.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[StudioCollections] Erreur lors de l'injection du script");
            }
        }

        public void Dispose() { }
    }
}
