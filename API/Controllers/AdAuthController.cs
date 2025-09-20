using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using API.Services;
using API.DTOs;

namespace API.Controllers
{
    [ApiController]
    [Route("api/adauth")]
    public class AdAuthController : ControllerBase
    {
        private readonly AdLdapAuthService _service;
        public AdAuthController(AdLdapAuthService service)
        {
            _service = service;
        }

        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<ActionResult<AdLoginResponseDto>> Login([FromBody] AdLoginRequestDto dto, CancellationToken ct)
        {
            try
            {
                var res = await _service.LoginWithAdAsync(dto.Username, dto.Password, ct);
                return Ok(res);
            }
            catch (UnauthorizedAccessException uae)
            {
                return Unauthorized(new { error = uae.Message });
            }
            catch (ArgumentException aex)
            {
                return BadRequest(new { error = aex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }
    }
}

