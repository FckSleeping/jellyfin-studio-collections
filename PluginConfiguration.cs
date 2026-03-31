using MediaBrowser.Model.Plugins;

namespace Jellyfin.Plugin.StudioCollections.Configuration
{
    public class PluginConfiguration : BasePluginConfiguration
    {
        /// <summary>
        /// Nombre maximum de studios à afficher.
        /// </summary>
        public int MaxStudios { get; set; } = 20;

        /// <summary>
        /// Nombre minimum de films/séries requis pour qu'un studio apparaisse.
        /// </summary>
        public int MinItemsPerStudio { get; set; } = 3;

        /// <summary>
        /// Afficher uniquement les studios ayant un logo disponible.
        /// </summary>
        public bool OnlyWithLogo { get; set; } = false;

        /// <summary>
        /// Activer l'effet hover avec l'intro/backdrop du studio.
        /// </summary>
        public bool EnableHoverEffect { get; set; } = true;

        /// <summary>
        /// Durée de l'animation hover en millisecondes.
        /// </summary>
        public int HoverAnimationDuration { get; set; } = 400;

        /// <summary>
        /// Titre de la section affiché dans l'interface.
        /// </summary>
        public string SectionTitle { get; set; } = "Par Studio";

        /// <summary>
        /// Insérer la section studios sur la page d'accueil.
        /// </summary>
        public bool InjectOnHomepage { get; set; } = true;

        /// <summary>
        /// Types de médias inclus (Movies, Series, ou les deux).
        /// </summary>
        public string MediaTypes { get; set; } = "Movies,Series";
    }
}
