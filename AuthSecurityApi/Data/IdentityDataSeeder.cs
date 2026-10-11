using AuthSecurityApi.Domain.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace AuthSecurityApi.Data
{
    public static class IdentityDataSeeder
    {
        public static async Task SeedAsync(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();

            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<ApplicationRole>>();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var context = scope.ServiceProvider.GetRequiredService<AppIdentityDBContext>();

            // The starter project uses EnsureCreated rather than EF migrations.
            // EnsureCreated does not update an existing database, so add the new columns idempotently.
            await context.Database.EnsureCreatedAsync();
            await EnsureModule13ColumnsAsync(context);
            await context.Database.ExecuteSqlRawAsync(
                "UPDATE [dbo].[AspNetUsers] SET [LockoutEnabled] = 1 WHERE [LockoutEnabled] = 0;");

            string[] roles = ["Admin", "Manager", "Customer"];
            foreach (var roleName in roles)
            {
                if (!await roleManager.RoleExistsAsync(roleName))
                {
                    var roleResult = await roleManager.CreateAsync(
                        new ApplicationRole(roleName, $"Системна роль {roleName}"));
                    if (!roleResult.Succeeded)
                    {
                        throw new InvalidOperationException(
                            $"Не вдалося створити роль {roleName}: {string.Join(", ", roleResult.Errors.Select(x => x.Description))}");
                    }
                }
            }

            const string adminEmail = "admin@authsystem.local";
            if (await userManager.FindByEmailAsync(adminEmail) == null)
            {
                var adminUser = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    FirstName = "Головний",
                    LastName = "Адміністратор",
                    EmailConfirmed = true,
                    IsActive = true,
                    LockoutEnabled = true
                };

                var result = await userManager.CreateAsync(adminUser, "Admin123!");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(adminUser, "Admin");
                }
                else
                {
                    throw new InvalidOperationException(
                        $"Не вдалося створити адміністратора: {string.Join(", ", result.Errors.Select(x => x.Description))}");
                }
            }
        }

        private static Task<int> EnsureModule13ColumnsAsync(AppIdentityDBContext context)
        {
            return context.Database.ExecuteSqlRawAsync("""
                IF COL_LENGTH(N'dbo.AspNetUsers', N'DateOfBirth') IS NULL
                    ALTER TABLE [dbo].[AspNetUsers] ADD [DateOfBirth] date NULL;

                IF COL_LENGTH(N'dbo.AspNetUsers', N'City') IS NULL
                    ALTER TABLE [dbo].[AspNetUsers] ADD [City] nvarchar(100) NULL;

                IF COL_LENGTH(N'dbo.AspNetUsers', N'FailedLoginCount') IS NULL
                    ALTER TABLE [dbo].[AspNetUsers]
                    ADD [FailedLoginCount] int NOT NULL
                    CONSTRAINT [DF_AspNetUsers_FailedLoginCount] DEFAULT (0);
                """);
        }
    }
}
