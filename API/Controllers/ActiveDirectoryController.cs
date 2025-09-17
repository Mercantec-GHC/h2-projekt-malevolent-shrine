using API.AD;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // Требует авторизации для использования AD методов
    public class ActiveDirectoryController : ControllerBase
    {
        private readonly ActiveDirectoryService _adService;

        public ActiveDirectoryController(ActiveDirectoryService adService)
        {
            _adService = adService;
        }

        [HttpGet("groups")]
        public async Task<IActionResult> GetAllGroups()
        {
            try
            {
                var groups = _adService.GetAllGroups();
                return Ok(groups);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpGet("users")]
        public async Task<IActionResult> GetAllUsers()
        {
            try
            {
                var users = _adService.GetAllUsers();
                return Ok(users);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpGet("users/search/{searchTerm}")]
        public async Task<IActionResult> SearchUsers(string searchTerm)
        {
            try
            {
                var users = _adService.SearchUsers(searchTerm);
                return Ok(users);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpPost("test-connection")]
        public async Task<IActionResult> TestConnection()
        {
            try
            {
                _adService.TestConnection();
                return Ok(new { message = "Connection successful" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }
    }
}