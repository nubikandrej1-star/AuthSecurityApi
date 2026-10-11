using Microsoft.AspNetCore.Identity;

namespace AuthSecurityApi.Domain.Models
{
    public class ApplicationRole : IdentityRole<int>
    {
        public string Description { get; set; } = string.Empty;

        public ApplicationRole() : base() { }

        public ApplicationRole(string roleName, string description = "") : base(roleName)
        {
            Description = description;
        }
    }
}
