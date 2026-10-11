using AuthSecurityApi.Domain.DTOs;
using AuthSecurityApi.Domain.Models;
using AuthSecurityApi.Security;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace AuthSecurityApi.Service
{
    public class UserService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<UserService> _logger;

        public UserService(UserManager<ApplicationUser> userManager, ILogger<UserService> logger)
        {
            _userManager = userManager;
            _logger = logger;
        }

        public async Task<IReadOnlyList<UserProfileResponse>> GetAllProfilesAsync()
        {
            var users = await _userManager.Users.OrderBy(user => user.Id).ToListAsync();
            return users.Select(MapProfile).ToList();
        }

        public async Task<UserProfileResponse?> GetProfileAsync(int userId)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            return user == null ? null : MapProfile(user);
        }

        public async Task<(UserProfileResponse? Profile, string[] Errors)> UpdateOwnProfileAsync(
            int userId, UpdateProfileRequest request)
        {
            if (request.DateOfBirth.HasValue && !AgeRestriction.IsAtLeastSixteen(request.DateOfBirth.Value))
            {
                return (null, ["Користувач має бути віком щонайменше 16 років."]);
            }

            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null)
                return (null, ["Користувача не знайдено."]);

            user.FirstName = request.FirstName.Trim();
            user.LastName = request.LastName.Trim();
            user.City = string.IsNullOrWhiteSpace(request.City) ? null : request.City.Trim();
            user.PhoneNumber = string.IsNullOrWhiteSpace(request.PhoneNumber) ? null : request.PhoneNumber.Trim();
            user.DateOfBirth = request.DateOfBirth;

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                return (null, result.Errors.Select(error => error.Description).ToArray());
            }

            _logger.LogInformation("Користувач {UserId} оновив свій профіль", user.Id);
            return (MapProfile(user), []);
        }

        public async Task<(bool Success, string Message)> LockUserAsync(int userId, int lockoutMinutes)
        {
            if (lockoutMinutes is < 1 or > 525600)
                return (false, "Тривалість блокування має бути від 1 хвилини до 365 днів.");

            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null)
                return (false, "Користувача не знайдено.");

            var enableLockoutResult = await _userManager.SetLockoutEnabledAsync(user, true);
            if (!enableLockoutResult.Succeeded)
            {
                return (false, string.Join("; ", enableLockoutResult.Errors.Select(error => error.Description)));
            }

            var lockoutEnd = DateTimeOffset.UtcNow.AddMinutes(lockoutMinutes);
            var result = await _userManager.SetLockoutEndDateAsync(user, lockoutEnd);
            if (!result.Succeeded)
                return (false, string.Join("; ", result.Errors.Select(error => error.Description)));

            _logger.LogWarning("Адміністратор заблокував користувача {UserId} до {LockoutEnd}", user.Id, lockoutEnd);
            return (true, $"Обліковий запис заблоковано до {lockoutEnd:O}.");
        }

        public async Task<(bool Success, string Message)> UnlockUserAsync(int userId)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null)
                return (false, "Користувача не знайдено.");

            var lockoutResult = await _userManager.SetLockoutEndDateAsync(user, null);
            if (!lockoutResult.Succeeded)
                return (false, string.Join("; ", lockoutResult.Errors.Select(error => error.Description)));

            var resetResult = await _userManager.ResetAccessFailedCountAsync(user);
            if (!resetResult.Succeeded)
                return (false, string.Join("; ", resetResult.Errors.Select(error => error.Description)));

            user.FailedLoginCount = 0;
            var updateResult = await _userManager.UpdateAsync(user);
            if (!updateResult.Succeeded)
                return (false, string.Join("; ", updateResult.Errors.Select(error => error.Description)));

            _logger.LogInformation("Адміністратор розблокував користувача {UserId}", user.Id);
            return (true, "Обліковий запис розблоковано.");
        }

        private static UserProfileResponse MapProfile(ApplicationUser user) => new()
        {
            Id = user.Id,
            Email = user.Email ?? string.Empty,
            FirstName = user.FirstName,
            LastName = user.LastName,
            FullName = user.FullName,
            PhoneNumber = user.PhoneNumber,
            City = user.City,
            DateOfBirth = user.DateOfBirth,
            FailedLoginCount = user.FailedLoginCount,
            IsActive = user.IsActive,
            IsLockedOut = user.LockoutEnabled && user.LockoutEnd.HasValue && user.LockoutEnd.Value > DateTimeOffset.UtcNow,
            CreatedAtUtc = user.CreatedAtUtc,
            LastLoginAtUtc = user.LastLoginAtUtc
        };
    }
}
