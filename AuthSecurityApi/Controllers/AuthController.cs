using System.Security.Claims;
using AuthSecurityApi.Domain.DTOs;
using AuthSecurityApi.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AuthSecurityApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class AuthController : ControllerBase
    {
        private readonly AuthService _service;

        public AuthController(AuthService service)
        {
            _service = service;
        }

        [AllowAnonymous]
        [HttpPost("register")]
        [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status409Conflict)]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            var result = await _service.RegistryAsync(request);
            if (!result.Success)
            {
                return result.ErrorCode == "duplicate_email"
                    ? Conflict(result)
                    : BadRequest(result);
            }

            return StatusCode(StatusCodes.Status201Created, result);
        }

        [AllowAnonymous]
        [HttpPost("login")]
        [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            var result = await _service.LogInAsync(request);
            if (result.Success)
                return Ok(result);

            return result.ErrorCode switch
            {
                "account_locked" or "account_inactive" => StatusCode(StatusCodes.Status403Forbidden, result),
                _ => Unauthorized(result)
            };
        }

        [Authorize]
        [HttpPost("logout")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> Logout()
        {
            await _service.LogOutAsync();
            return NoContent();
        }

        [Authorize]
        [HttpPost("change-password")]
        [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
        {
            var claimId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(claimId, out var userId))
                return Unauthorized();

            var result = await _service.ChangePasswordAsync(userId, request);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        /// <summary>Створює токен відновлення пароля. У Development токен повертається для тестування.</summary>
        [AllowAnonymous]
        [HttpPost("forgot-password")]
        [ProducesResponseType(typeof(ForgotPasswordResponse), StatusCodes.Status200OK)]
        public async Task<ActionResult<ForgotPasswordResponse>> ForgotPassword([FromBody] ForgotPasswordRequest request)
        {
            var response = await _service.ForgotPasswordAsync(request);
            return Ok(response);
        }

        /// <summary>Скидає пароль за одноразовим токеном Identity.</summary>
        [AllowAnonymous]
        [HttpPost("reset-password")]
        [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
        {
            var result = await _service.ResetPasswordAsync(request);
            return result.Success ? Ok(result) : BadRequest(result);
        }
    }
}
