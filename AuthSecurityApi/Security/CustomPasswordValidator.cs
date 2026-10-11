using AuthSecurityApi.Domain.Models;
using Microsoft.AspNetCore.Identity;

namespace AuthSecurityApi.Security
{
    public sealed class CustomPasswordValidator<TUser> : IPasswordValidator<TUser>
        where TUser : ApplicationUser
    {
        private static readonly HashSet<string> WeakPasswords = new(StringComparer.OrdinalIgnoreCase)
        {
            "password",
            "admin123",
            "qwerty123",
            "12345678"
        };

        public Task<IdentityResult> ValidateAsync(
            UserManager<TUser> manager,
            TUser user,
            string? password)
        {
            var errors = new List<IdentityError>();

            if (string.IsNullOrWhiteSpace(password))
            {
                errors.Add(new IdentityError
                {
                    Code = "PasswordEmpty",
                    Description = "Пароль не може бути порожнім."
                });
                return Task.FromResult(IdentityResult.Failed(errors.ToArray()));
            }

            if (WeakPasswords.Contains(password))
            {
                errors.Add(new IdentityError
                {
                    Code = "WeakPassword",
                    Description = "Цей пароль надто поширений. Оберіть інший пароль."
                });
            }

            if (ContainsPersonalValue(password, user.FirstName) ||
                ContainsPersonalValue(password, user.LastName))
            {
                errors.Add(new IdentityError
                {
                    Code = "PasswordContainsName",
                    Description = "Пароль не повинен містити ім'я або прізвище користувача."
                });
            }

            if (user.DateOfBirth.HasValue)
            {
                var birthYear = user.DateOfBirth.Value.Year.ToString();
                if (password.Contains(birthYear, StringComparison.OrdinalIgnoreCase))
                {
                    errors.Add(new IdentityError
                    {
                        Code = "PasswordContainsBirthYear",
                        Description = "Пароль не повинен містити рік народження користувача."
                    });
                }
            }

            return Task.FromResult(errors.Count == 0
                ? IdentityResult.Success
                : IdentityResult.Failed(errors.ToArray()));
        }

        private static bool ContainsPersonalValue(string password, string? personalValue)
        {
            var value = personalValue?.Trim();
            return !string.IsNullOrWhiteSpace(value) &&
                   password.Contains(value, StringComparison.OrdinalIgnoreCase);
        }
    }
}
