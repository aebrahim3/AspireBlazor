using CmsBackend.Models;
using Microsoft.AspNetCore.Identity;

namespace CmsBackend.Data;

public static class DbSeeder
{
    public static async Task SeedAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

        // Ensure roles exist
        var roles = new[] { "Admin", "Writer" };
        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
            }
        }

        var writerUserName = "writer";
        var writerEmail = "w@w.w";
        var writerPassword = "P@$$w0rd";

        var writer = await userManager.FindByEmailAsync(writerEmail);
        if (writer == null)
        {
            writer = new IdentityUser
            {
                UserName = writerUserName,
                Email = writerEmail,
                EmailConfirmed = true,
            };
            await userManager.CreateAsync(writer, writerPassword);
        }
        if (!await userManager.IsInRoleAsync(writer, "Writer"))
        {
            await userManager.AddToRoleAsync(writer, "Writer");
        }

        var adminUserName = "admin";
        var adminEmail = "a@a.a";
        var adminPassword = "P@$$w0rd";

        var admin = await userManager.FindByEmailAsync(adminEmail);
        if (admin == null)
        {
            admin = new IdentityUser
            {
                UserName = adminUserName,
                Email = adminEmail,
                EmailConfirmed = true,
            };
            await userManager.CreateAsync(admin, adminPassword);
        }
        if (!await userManager.IsInRoleAsync(admin, "Admin"))
        {
            await userManager.AddToRoleAsync(admin, "Admin");
        }

        if (!db.Articles.Any())
        {
            db.Articles.AddRange(
                new Article
                {
                    Title = "Welcome",
                    ContentHtml = "<p>Seeded article 1.</p>",
                    AuthorName = "admin",
                },
                new Article
                {
                    Title = "About",
                    ContentHtml = "<p>Seeded article 2.</p>",
                    AuthorName = "writer",
                },
                new Article
                {
                    Title = "FAQ",
                    ContentHtml = "<p>Seeded article 3.</p>",
                    AuthorName = "admin",
                },
                new Article
                {
                    Title = "News",
                    ContentHtml = "<p>Seeded article 4.</p>",
                    AuthorName = "writer",
                },
                new Article
                {
                    Title = "Getting Started",
                    ContentHtml = "<p>Seeded article 5.</p>",
                    AuthorName = "admin",
                },
                new Article
                {
                    Title = "Contact",
                    ContentHtml = "<p>Seeded article 6.</p>",
                    AuthorName = "admin",
                }
            );
            await db.SaveChangesAsync();
        }
    }
}
