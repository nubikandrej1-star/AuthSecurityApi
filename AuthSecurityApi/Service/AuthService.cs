using AuthSecurityApi.Domain.DTOs;
using AuthSecurityApi.Domain.Models;
using AuthSecurityApi.Security;
using Microsoft.AspNetCore.Identity;

namespace AuthSecurityApi.Service
{
    public class AuthService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly RoleManager<ApplicationRole> _roleManager;
        private readonly ILogger<AuthService> _logger;
        private readonly IWebHostEnvironment _environment;

        public AuthService(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            RoleManager<ApplicationRole> roleManager,
            ILogger<AuthService> logger,
            IWebHostEnvironment environment)
        {
            _logger = logger;
            _userManager = userManager;
            _roleManager = roleManager;
            _signInManager = signInManager;
            _environment = environment;
        }

        public async Task<AuthResponse> RegistryAsync(RegisterRequest request)
        {
            if (request.DateOfBirth.HasValue &&
                !AgeRestriction.IsAtLeastSixteen(request.DateOfBirth.Value))
            {
                return AuthResponse.Fail("Для реєстрації потрібно мати щонайменше 16 років.",
                    errorCode: "age_restriction");
            }

            var existingUser = await _userManager.FindByEmailAsync(request.Email.Trim());
            if (existingUser != null)
                return AuthResponse.Fail("Користувач із таким Email уже існує.", errorCode: "duplicate_email");

            var user = new ApplicationUser
            {
                FirstName = request.FirstName.Trim(),
                LastName = request.LastName.Trim(),
                DateOfBirth = request.DateOfBirth,
                City = string.IsNullOrWhiteSpace(request.City) ? null : request.City.Trim(),
                UserName = request.Email.Trim(),
                Email = request.Email.Trim(),
                PhoneNumber = request.PhoneNumber,
                IsActive = true,
                EmailConfirmed = true,
                LockoutEnabled = true
            };

            var createResult = await _userManager.CreateAsync(user, request.Password);
            if (!createResult.Succeeded)
            {
                return AuthResponse.Fail("Не вдалося зареєструвати користувача.",
                    createResult.Errors.Select(error => error.Description));
            }

            if (await _roleManager.RoleExistsAsync("Customer"))
            {
                var roleResult = await _userManager.AddToRoleAsync(user, "Customer");
                if (!roleResult.Succeeded)
                {
                    _logger.LogError("Не вдалося призначити роль Customer користувачу {UserId}", user.Id);
                }
            }

            var roles = await _userManager.GetRolesAsync(user);
            return AuthResponse.Ok(user.Id, user.Email, user.FullName, roles, "Реєстрація успішна.");
        }

        public async Task<AuthResponse> LogInAsync(LoginRequest loginRequest)
        {
            var user = await _userManager.FindByEmailAsync(loginRequest.Email.Trim());
            if (user == null)
                return AuthResponse.Fail("Неправильний Email або пароль.", errorCode: "invalid_credentials");

            if (!user.IsActive)
                return AuthResponse.Fail("Обліковий запис неактивний.", errorCode: "account_inactive");

            if (await _userManager.IsLockedOutAsync(user))
            {
                return AuthResponse.Fail(GetLockoutMessage(user), errorCode: "account_locked");
            }

            var signInResult = await _signInManager.CheckPasswordSignInAsync(
                user, loginRequest.Password, lockoutOnFailure: true);

            if (!signInResult.Succeeded)
            {
                var refreshedUser = await _userManager.FindByIdAsync(user.Id.ToString()) ?? user;
                refreshedUser.FailedLoginCount++;
                await _userManager.UpdateAsync(refreshedUser);

                _logger.LogWarning("Невдала спроба входу для користувача {UserId}. Кількість: {Count}",
                    refreshedUser.Id, refreshedUser.FailedLoginCount);

                if (signInResult.IsLockedOut || await _userManager.IsLockedOutAsync(refreshedUser))
                {
                    return AuthResponse.Fail(GetLockoutMessage(refreshedUser), errorCode: "account_locked");
                }

                return AuthResponse.Fail("Неправильний Email або пароль.", errorCode: "invalid_credentials");
            }

            user.FailedLoginCount = 0;
            user.LastLoginAtUtc = DateTime.UtcNow;
            var updateResult = await _userManager.UpdateAsync(user);
            if (!updateResult.Succeeded)
            {
                _logger.LogError("Не вдалося оновити дані після входу для користувача {UserId}", user.Id);
                return AuthResponse.Fail("Не вдалося завершити вхід. Спробуйте ще раз.");
            }

            // CheckPasswordSignInAsync verifies the password but does not issue the authentication cookie.
            await _signInManager.SignInAsync(user, isPersistent: false);

            var roles = await _userManager.GetRolesAsync(user);
            return AuthResponse.Ok(user.Id, user.Email, user.FullName, roles, "Вхід успішний.");
        }

        public async Task<bool> LogOutAsync()
        {
            await _signInManager.SignOutAsync();
            return true;
        }

        public async Task<AuthResponse> ChangePasswordAsync(int userId, ChangePasswordRequest request)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null)
                return AuthResponse.Fail("Користувача не знайдено.");

            var result = await _userManager.ChangePasswordAsync(user, request.CurrentPassword, request.NewPassword);
            if (!result.Succeeded)
            {
                return AuthResponse.Fail("Не вдалося змінити пароль.",
                    result.Errors.Select(error => error.Description));
            }

            return AuthResponse.Ok(user.Id, user.Email, user.FullName,
                await _userManager.GetRolesAsync(user), "Пароль успішно змінено.");
        }

        public async Task<ForgotPasswordResponse> ForgotPasswordAsync(ForgotPasswordRequest request)
        {
            // The response is identical for existing and non-existing accounts to prevent email enumeration.
            var response = new ForgotPasswordResponse();
            var user = await _userManager.FindByEmailAsync(request.Email.Trim());
            if (user == null)
                return response;

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            if (_environment.IsDevelopment())
            {
                // Test-only behavior required by this homework; production must deliver this through email.
                response.Token = token;
            }
            else
            {
                _logger.LogInformation("Запит на відновлення пароля створено для користувача {UserId}", user.Id);
            }

            return response;
        }

        public async Task<AuthResponse> ResetPasswordAsync(ResetPasswordRequest request)
        {
            var user = await _userManager.FindByEmailAsync(request.Email.Trim());
            if (user == null)
            {
                return AuthResponse.Fail("Неправильний Email або токен скидання пароля.",
                    errorCode: "invalid_reset_request");
            }

            var result = await _userManager.ResetPasswordAsync(user, request.Token, request.NewPassword);
            if (!result.Succeeded)
            {
                return AuthResponse.Fail("Не вдалося скинути пароль.",
                    result.Errors.Select(error => error.Description));
            }

            user.FailedLoginCount = 0;
            var updateResult = await _userManager.UpdateAsync(user);
            if (!updateResult.Succeeded)
            {
                return AuthResponse.Fail("Пароль скинуто, але не вдалося оновити аудит входів.",
                    updateResult.Errors.Select(error => error.Description));
            }

            return AuthResponse.Ok(user.Id, user.Email, user.FullName,
                await _userManager.GetRolesAsync(user), "Пароль успішно скинуто.");
        }

        private static string GetLockoutMessage(ApplicationUser user)
        {
            if (user.LockoutEnd.HasValue)
            {
                var remaining = user.LockoutEnd.Value - DateTimeOffset.UtcNow;
                if (remaining > TimeSpan.Zero)
                {
                    return $"Обліковий запис заблоковано. Спробуйте через {Math.Ceiling(remaining.TotalMinutes)} хв.";
                }
            }

            return "Обліковий запис заблоковано. Зверніться до адміністратора.";
        }
    }
}
