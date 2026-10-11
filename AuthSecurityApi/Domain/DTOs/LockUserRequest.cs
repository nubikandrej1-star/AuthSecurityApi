using System.ComponentModel.DataAnnotations;

namespace AuthSecurityApi.Domain.DTOs
{
    public class LockUserRequest
    {
        [Range(1, 525600, ErrorMessage = "Тривалість блокування має бути від 1 хвилини до 365 днів")]
        public int LockoutMinutes { get; set; } = 30;
    }
}
