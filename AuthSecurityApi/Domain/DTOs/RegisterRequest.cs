using System.ComponentModel.DataAnnotations;

namespace AuthSecurityApi.Domain.DTOs
{
    public class RegisterRequest
    {
        [Required(ErrorMessage = "Ім'я є обов'язковим")]
        [StringLength(100, ErrorMessage = "Ім'я не може перевищувати 100 символів")]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Прізвище є обов'язковим")]
        [StringLength(100, ErrorMessage = "Прізвище не може перевищувати 100 символів")]
        public string LastName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email є обов'язковим")]
        [EmailAddress(ErrorMessage = "Некоректний формат Email")]
        public string Email { get; set; } = string.Empty;

        [Phone(ErrorMessage = "Некоректний формат номера телефону")]
        public string? PhoneNumber { get; set; }

        [StringLength(100, ErrorMessage = "Місто не може перевищувати 100 символів")]
        public string? City { get; set; }

        public DateOnly? DateOfBirth { get; set; }

        [Required(ErrorMessage = "Пароль є обов'язковим")]
        [MinLength(8, ErrorMessage = "Пароль повинен містити щонайменше 8 символів")]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Підтвердження пароля є обов'язковим")]
        [Compare(nameof(Password), ErrorMessage = "Паролі не співпадають")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}
