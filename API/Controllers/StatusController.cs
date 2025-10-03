using Microsoft.AspNetCore.Mvc;
using API.Data; 

namespace API.Controllers
{
    /// <summary>
    /// Simple health and ping endpoints for the API and database connectivity.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class StatusController : ControllerBase
    {
        private readonly AppDBContext _context;

        /// <summary>
        /// Injects the application DbContext.
        /// </summary>
        public StatusController(AppDBContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Checks whether the API is running.
        /// </summary>
        /// <returns>Status object indicating the API is healthy.</returns>
        /// <response code="200">API is running.</response>
        [HttpGet("healthcheck")]
        public IActionResult HealthCheck()
        {
            return Ok(new { status = "OK", message = "API is running" });
        }

        /// <summary>
        /// Checks whether the database is reachable via EF Core.
        /// </summary>
        /// <returns>Status object indicating database connectivity.</returns>
        /// <response code="200">Database connection succeeded.</response>
        /// <response code="500">Database connection failed.</response>
        [HttpGet("dbhealthcheck")]
        public IActionResult DBHealthCheck()
        {
            try
            {
                if (_context.Database.CanConnect())
                {
                    return Ok(new { status = "OK", message = "Database is available" });
                }
                else
                {
                    return StatusCode(500, new { status = "Error", message = "Cannot connect to the database." });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { status = "Error", message = "Database connection error: " + ex.Message });
            }
        }

        /// <summary>
        /// Simple ping endpoint to test API responsiveness.
        /// </summary>
        /// <returns>Status object with a pong message.</returns>
        /// <response code="200">API responded with Pong.</response>
        [HttpGet("ping")]
        public IActionResult Ping()
        {
            return Ok(new { status = "OK", message = "Pong 🏓" });
        }
    }
}