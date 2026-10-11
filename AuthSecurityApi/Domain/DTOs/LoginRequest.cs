using System.ComponentModel.DataAnnotations;

namespace AuthSecurityApi.Domain.DTOs
{
    public class LoginRequest
    {
        [Required(ErrorMessage = "Email є обов'язковим")]
        [EmailAddress(ErrorMessage = "Некоректний формат Email")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Пароль є обов'язковим")]
        public string Password { get; set; } = string.Empty;
    }
}
