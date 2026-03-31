using System.IO;
using System.Reflection;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Jellyfin.Plugin.StudioCollections.Controllers
{
    /// <summary>
    /// Sert le script JS client comme ressource embarquée.
    /// Endpoint : GET /StudioCollections/clientscript
    /// </summary>
    [ApiController]
    [Route("StudioCollections")]
    public class ClientScriptController : ControllerBase
    {
        [HttpGet("clientscript")]
        [AllowAnonymous]
        [Produces("application/javascript")]
        public IActionResult GetScript()
        {
            var asm = Assembly.GetExecutingAssembly();
            const string resourceName = "Jellyfin.Plugin.StudioCollections.Web.studioCollections.js";
            var stream = asm.GetManifestResourceStream(resourceName);

            if (stream == null)
                return NotFound("Script introuvable");

            return File(stream, "application/javascript");
        }
    }
}
