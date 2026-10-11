using System.ComponentModel.DataAnnotations;

namespace AuthSecurityApi.Domain.DTOs
{
    public class ChangePasswordRequest
    {
        [Required(ErrorMessage = "Поточний пароль є обов'язковим")]
        public string CurrentPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "Новий пароль є обов'язковим")]
        [MinLength(8, ErrorMessage = "Пароль повинен містити щонайменше 8 символів")]
        public string NewPassword { get; set; } = string.Empty;
    }
}
