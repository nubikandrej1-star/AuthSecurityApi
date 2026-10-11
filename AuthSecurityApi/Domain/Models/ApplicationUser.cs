using Microsoft.AspNetCore.Identity;

namespace AuthSecurityApi.Domain.Models
{
    public class ApplicationUser : IdentityUser<int>
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public DateOnly? DateOfBirth { get; set; }
        public string? City { get; set; }
        public int FailedLoginCount { get; set; }
        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
        public bool IsActive { get; set; } = true;
        public DateTime? LastLoginAtUtc { get; set; }

        public string FullName => $"{FirstName} {LastName}".Trim();
    }
}
