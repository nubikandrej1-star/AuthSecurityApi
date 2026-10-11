using AuthSecurityApi.Domain.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace AuthSecurityApi.Data
{
    public class AppIdentityDBContext : IdentityDbContext<ApplicationUser, ApplicationRole, int>
    {
        public AppIdentityDBContext(DbContextOptions<AppIdentityDBContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<ApplicationUser>(entity =>
            {
                entity.Property(user => user.FirstName).HasMaxLength(100).IsRequired();
                entity.Property(user => user.LastName).HasMaxLength(100).IsRequired();
                entity.Property(user => user.DateOfBirth).HasColumnType("date");
                entity.Property(user => user.City).HasMaxLength(100);
                entity.Property(user => user.FailedLoginCount).HasDefaultValue(0);
            });

            builder.Entity<ApplicationRole>(entity =>
            {
                entity.Property(role => role.Description).HasMaxLength(150);
            });
        }
    }
}
