using Microsoft.AspNetCore.Mvc;
using API.Services;


namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DataSeederController : ControllerBase
    {
        private readonly DataSeederService _dataSeederService;
        private readonly ILogger<DataSeederController> _logger;

        public DataSeederController(DataSeederService dataSeederService, ILogger<DataSeederController> logger)
        {
            _dataSeederService = dataSeederService;
            _logger = logger;
        }
        

        [HttpPost("seed-users")]
        public async Task<IActionResult> SeedUsers([FromQuery] int count = 50)
        {
            _logger.LogInformation("Запрос на создание {Count} пользователей.", count);

            var users = await _dataSeederService.SeedUsersAsync(count);

            _logger.LogInformation("Создано {Count} пользователей.", users.Count);
    
            return Ok(new { Message = $"{users.Count} пользователей успешно создано." });
        }
        
        [HttpPost("seed-all")]
        public async Task<IActionResult> SeedAllData([FromQuery] int count = 50)
        {
            _logger.LogInformation("Начинаем заполнение базы данных.");

            await _dataSeederService.SeedUsersAsync(count);
            await _dataSeederService.SeedHotelsAsync(count);

            _logger.LogInformation("Заполнение базы данных завершено.");

            return Ok(new { Message = $"База данных успешно заполнена {count} пользователями и отелями." });
        }
        
        
    }
}