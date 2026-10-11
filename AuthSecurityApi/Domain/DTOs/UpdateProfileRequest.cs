using System.ComponentModel.DataAnnotations;

namespace AuthSecurityApi.Domain.DTOs
{
    public class UpdateProfileRequest
    {
        [Required(ErrorMessage = "Ім'я є обов'язковим")]
        [StringLength(100, ErrorMessage = "Ім'я не може перевищувати 100 символів")]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Прізвище є обов'язковим")]
        [StringLength(100, ErrorMessage = "Прізвище не може перевищувати 100 символів")]
        public string LastName { get; set; } = string.Empty;

        [StringLength(100, ErrorMessage = "Місто не може перевищувати 100 символів")]
        public string? City { get; set; }

        [Phone(ErrorMessage = "Некоректний формат номера телефону")]
        public string? PhoneNumber { get; set; }

        public DateOnly? DateOfBirth { get; set; }
    }
}
