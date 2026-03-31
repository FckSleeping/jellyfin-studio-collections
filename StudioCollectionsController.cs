using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Jellyfin.Plugin.StudioCollections.Configuration;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Entities.Movies;
using MediaBrowser.Controller.Entities.TV;
using MediaBrowser.Controller.Library;
using MediaBrowser.Model.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Plugin.StudioCollections.Controllers
{
    /// <summary>
    /// Modèle de réponse pour un studio.
    /// </summary>
    public class StudioDto
    {
        public string Name { get; set; } = string.Empty;
        public string? LogoUrl { get; set; }
        public string? BackdropUrl { get; set; }
        public string? ThumbUrl { get; set; }
        public int ItemCount { get; set; }
        public string BrowseUrl { get; set; } = string.Empty;
        public List<StudioItemDto> RecentItems { get; set; } = new();
    }

    public class StudioItemDto
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? PrimaryImageUrl { get; set; }
        public string? BackdropUrl { get; set; }
        public string Type { get; set; } = string.Empty;
        public int? ProductionYear { get; set; }
    }

    [ApiController]
    [Route("StudioCollections")]
    public class StudioCollectionsController : ControllerBase
    {
        private readonly ILibraryManager _libraryManager;
        private readonly ILogger<StudioCollectionsController> _logger;

        public StudioCollectionsController(
            ILibraryManager libraryManager,
            ILogger<StudioCollectionsController> logger)
        {
            _libraryManager = libraryManager;
            _logger = logger;
        }

        /// <summary>
        /// Récupère la liste des studios avec leurs métadonnées.
        /// GET /StudioCollections/studios
        /// </summary>
        [HttpGet("studios")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public ActionResult<IEnumerable<StudioDto>> GetStudios()
        {
            try
            {
                var config = Plugin.Instance?.Configuration ?? new PluginConfiguration();
                var mediaTypes = config.MediaTypes.Split(',', StringSplitOptions.RemoveEmptyEntries);

                // Récupérer tous les items correspondant aux types configurés
                var queryResult = _libraryManager.GetItemList(new MediaBrowser.Controller.Entities.InternalItemsQuery
                {
                    IncludeItemTypes = GetItemTypes(mediaTypes),
                    Recursive = true,
                    IsVirtualItem = false,
                    HasTmdbId = true
                });

                // Grouper par studio
                var studioGroups = queryResult
                    .Where(item => item.Studios != null && item.Studios.Length > 0)
                    .SelectMany(item => item.Studios.Select(studio => new { Studio = studio, Item = item }))
                    .GroupBy(x => x.Studio, StringComparer.OrdinalIgnoreCase)
                    .Where(g => g.Count() >= config.MinItemsPerStudio)
                    .OrderByDescending(g => g.Count())
                    .Take(config.MaxStudios);

                var results = new List<StudioDto>();

                foreach (var group in studioGroups)
                {
                    // Chercher l'entité Studio dans Jellyfin
                    var studioEntity = _libraryManager.GetStudio(group.Key);

                    // Construire les URLs d'images
                    string? logoUrl = null;
                    string? backdropUrl = null;
                    string? thumbUrl = null;

                    if (studioEntity != null)
                    {
                        var studioId = studioEntity.Id.ToString("N");

                        if (studioEntity.HasImage(ImageType.Logo, 0))
                            logoUrl = $"/Items/{studioId}/Images/Logo";
                        if (studioEntity.HasImage(ImageType.Backdrop, 0))
                            backdropUrl = $"/Items/{studioId}/Images/Backdrop";
                        if (studioEntity.HasImage(ImageType.Thumb, 0))
                            thumbUrl = $"/Items/{studioId}/Images/Thumb";
                        if (thumbUrl == null && studioEntity.HasImage(ImageType.Primary, 0))
                            thumbUrl = $"/Items/{studioId}/Images/Primary";
                    }

                    if (config.OnlyWithLogo && logoUrl == null)
                        continue;

                    // Items récents du studio (max 10)
                    var recentItems = group
                        .OrderByDescending(x => x.Item.DateCreated)
                        .Take(10)
                        .Select(x =>
                        {
                            var itemId = x.Item.Id.ToString("N");
                            string? backdrop = x.Item.HasImage(ImageType.Backdrop, 0)
                                ? $"/Items/{itemId}/Images/Backdrop"
                                : null;
                            string? primary = x.Item.HasImage(ImageType.Primary, 0)
                                ? $"/Items/{itemId}/Images/Primary"
                                : null;

                            return new StudioItemDto
                            {
                                Id = itemId,
                                Name = x.Item.Name ?? string.Empty,
                                PrimaryImageUrl = primary,
                                BackdropUrl = backdrop,
                                Type = x.Item is Movie ? "Movie" : "Series",
                                ProductionYear = x.Item.ProductionYear
                            };
                        })
                        .ToList();

                    results.Add(new StudioDto
                    {
                        Name = group.Key,
                        LogoUrl = logoUrl,
                        BackdropUrl = backdropUrl ?? recentItems.FirstOrDefault()?.BackdropUrl,
                        ThumbUrl = thumbUrl,
                        ItemCount = group.Count(),
                        BrowseUrl = $"/web/#/list.html?studios={Uri.EscapeDataString(group.Key)}&genrefilter=false",
                        RecentItems = recentItems
                    });
                }

                return Ok(results);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération des studios");
                return StatusCode(500, "Erreur interne");
            }
        }

        /// <summary>
        /// Récupère les items d'un studio spécifique.
        /// GET /StudioCollections/studios/{studioName}/items
        /// </summary>
        [HttpGet("studios/{studioName}/items")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public ActionResult<IEnumerable<StudioItemDto>> GetStudioItems(string studioName, [FromQuery] int limit = 50)
        {
            try
            {
                var config = Plugin.Instance?.Configuration ?? new PluginConfiguration();
                var mediaTypes = config.MediaTypes.Split(',', StringSplitOptions.RemoveEmptyEntries);

                var items = _libraryManager.GetItemList(new MediaBrowser.Controller.Entities.InternalItemsQuery
                {
                    Studios = new[] { studioName },
                    IncludeItemTypes = GetItemTypes(mediaTypes),
                    Recursive = true,
                    IsVirtualItem = false,
                    Limit = limit
                });

                var result = items.Select(item =>
                {
                    var itemId = item.Id.ToString("N");
                    return new StudioItemDto
                    {
                        Id = itemId,
                        Name = item.Name ?? string.Empty,
                        PrimaryImageUrl = item.HasImage(ImageType.Primary, 0) ? $"/Items/{itemId}/Images/Primary" : null,
                        BackdropUrl = item.HasImage(ImageType.Backdrop, 0) ? $"/Items/{itemId}/Images/Backdrop" : null,
                        Type = item is Movie ? "Movie" : "Series",
                        ProductionYear = item.ProductionYear
                    };
                });

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération des items du studio {Studio}", studioName);
                return StatusCode(500, "Erreur interne");
            }
        }

        /// <summary>
        /// Retourne la configuration publique du plugin.
        /// GET /StudioCollections/config
        /// </summary>
        [HttpGet("config")]
        [AllowAnonymous]
        public ActionResult GetConfig()
        {
            var config = Plugin.Instance?.Configuration ?? new PluginConfiguration();
            return Ok(new
            {
                config.SectionTitle,
                config.EnableHoverEffect,
                config.HoverAnimationDuration,
                config.InjectOnHomepage
            });
        }

        private static string[] GetItemTypes(string[] mediaTypes)
        {
            var types = new List<string>();
            if (mediaTypes.Contains("Movies", StringComparer.OrdinalIgnoreCase))
                types.Add(nameof(Movie));
            if (mediaTypes.Contains("Series", StringComparer.OrdinalIgnoreCase))
                types.Add(nameof(Series));
            return types.Count > 0 ? types.ToArray() : new[] { nameof(Movie), nameof(Series) };
        }
    }
}
