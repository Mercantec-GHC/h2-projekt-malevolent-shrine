using Microsoft.AspNetCore.Mvc;
using API.Data;
using DomainModels.DTOs;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class StatusController : ControllerBase
    {
        private readonly AppDBContext _context;

        // Внедрение DbContext через DI
        public StatusController(AppDBContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Tjekker om API'en kører korrekt.
        /// </summary>
        /// <returns>Status og besked om API'ens tilstand.</returns>
        /// <response code="200">API'en er kørende.</response>
        [HttpGet("healthcheck")]
        public IActionResult HealthCheck()
        {
            return Ok(new { status = "OK", message = "API'en er kørende!" });
        }

        /// <summary>
        /// Tjekker om databasen er tilgængelig via EFCore.
        /// </summary>
        /// <returns>Status og besked om databaseforbindelse.</returns>
        /// <response code="200">Database er kørende.</response>
        /// <response code="500">Der er en fejl med databaseforbindelsen.</response>
        [HttpGet("dbhealthcheck")]
        public IActionResult DBHealthCheck()
        {
            try
            {
                if (_context.Database.CanConnect())
                {
                    return Ok(new { status = "OK", message = "Database er kørende!" });
                }
                else
                {
                    return StatusCode(500, new { status = "Error", message = "Kan ikke forbinde til databasen." });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { status = "Error", message = "Fejl ved forbindelse til database: " + ex.Message });
            }
        }

        /// <summary>
        /// Simpelt ping-endpoint til at teste API'en.
        /// </summary>
        /// <returns>Status og "Pong" besked.</returns>
        /// <response code="200">API'en svarede med Pong.</response>
        [HttpGet("ping")]
        public IActionResult Ping()
        {
            return Ok(new { status = "OK", message = "Pong 🏓" });
        }
    }
}
