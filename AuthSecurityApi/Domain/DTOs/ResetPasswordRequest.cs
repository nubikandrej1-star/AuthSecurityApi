using System.ComponentModel.DataAnnotations;

namespace AuthSecurityApi.Domain.DTOs
{
    public class ResetPasswordRequest
    {
        [Required(ErrorMessage = "Email є обов'язковим")]
        [EmailAddress(ErrorMessage = "Некоректний формат Email")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Токен скидання пароля є обов'язковим")]
        public string Token { get; set; } = string.Empty;

        [Required(ErrorMessage = "Новий пароль є обов'язковим")]
        [MinLength(8, ErrorMessage = "Пароль повинен містити щонайменше 8 символів")]
        public string NewPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "Підтвердження пароля є обов'язковим")]
        [Compare(nameof(NewPassword), ErrorMessage = "Паролі не співпадають")]
        public string ConfirmNewPassword { get; set; } = string.Empty;
    }
}
