using System.Security.Claims;
using AuthSecurityApi.Domain.DTOs;
using AuthSecurityApi.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AuthSecurityApi.Controllers
{
    [ApiController]
    [Route("api/users")]
    [Authorize]
    [Produces("application/json")]
    public class UsersController : ControllerBase
    {
        private readonly UserService _service;

        public UsersController(UserService service)
        {
            _service = service;
        }

        /// <summary>Повертає профілі користувачів. Доступно лише адміністраторам.</summary>
        [HttpGet]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(IReadOnlyList<UserProfileResponse>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetUsers()
        {
            return Ok(await _service.GetAllProfilesAsync());
        }

        /// <summary>Повертає профіль поточного користувача.</summary>
        [HttpGet("profile")]
        [ProducesResponseType(typeof(UserProfileResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetMyProfile()
        {
            if (!TryGetCurrentUserId(out var userId))
                return Unauthorized();

            var profile = await _service.GetProfileAsync(userId);
            return profile == null ? NotFound() : Ok(profile);
        }

        /// <summary>Повертає конкретний профіль. Звичайний користувач може переглядати лише власний профіль.</summary>
        [HttpGet("{id:int}")]
        [ProducesResponseType(typeof(UserProfileResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetUser(int id)
        {
            if (!User.IsInRole("Admin") &&
                (!TryGetCurrentUserId(out var currentUserId) || currentUserId != id))
            {
                return Forbid();
            }

            var profile = await _service.GetProfileAsync(id);
            return profile == null ? NotFound() : Ok(profile);
        }

        /// <summary>Оновлює особисті дані користувача без можливості змінити Email або пароль.</summary>
        [HttpPut("profile")]
        [ProducesResponseType(typeof(UserProfileResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> UpdateMyProfile([FromBody] UpdateProfileRequest request)
        {
            if (!TryGetCurrentUserId(out var userId))
                return Unauthorized();

            var (profile, errors) = await _service.UpdateOwnProfileAsync(userId, request);
            if (profile == null)
                return BadRequest(new { message = "Не вдалося оновити профіль.", errors });

            return Ok(profile);
        }

        /// <summary>Примусово блокує обліковий запис на вказану кількість хвилин. Доступно лише адміністраторам.</summary>
        [HttpPost("{id:int}/lock")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> LockUser(
            int id,
            [FromBody] LockUserRequest? request = null,
            [FromQuery] int? lockoutMinutes = null)
        {
            var minutes = lockoutMinutes ?? request?.LockoutMinutes ?? 30;
            var (success, message) = await _service.LockUserAsync(id, minutes);
            if (!success)
            {
                var statusCode = message == "Користувача не знайдено."
                    ? StatusCodes.Status404NotFound
                    : StatusCodes.Status400BadRequest;
                return StatusCode(statusCode, new { message });
            }

            return Ok(new { userId = id, lockoutMinutes = minutes, message });
        }

        /// <summary>Знімає блокування та обнуляє лічильник невдалих входів. Доступно лише адміністраторам.</summary>
        [HttpPost("{id:int}/unlock")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UnlockUser(int id)
        {
            var (success, message) = await _service.UnlockUserAsync(id);
            if (!success)
            {
                var statusCode = message == "Користувача не знайдено."
                    ? StatusCodes.Status404NotFound
                    : StatusCodes.Status400BadRequest;
                return StatusCode(statusCode, new { message });
            }

            return Ok(new { userId = id, message });
        }

        private bool TryGetCurrentUserId(out int userId)
        {
            return int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out userId);
        }
    }
}
