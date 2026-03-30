using AspireBlazor.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
namespace AspireBlazor.Data;

public static class DbSeeder
{
    public static async Task SeedAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        await db.Database.MigrateAsync();
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

        // Create first writer account (w@w.w)
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

        // Create second writer account (x@x.x) as required by assignment.
        var writer2UserName = "writer2";
        var writer2Email = "x@x.x";
        var writer2Password = "P@$$w0rd";

        var writer2 = await userManager.FindByEmailAsync(writer2Email);
        if (writer2 == null)
        {
            writer2 = new IdentityUser
            {
                UserName = writer2UserName,
                Email = writer2Email,
                EmailConfirmed = true,
            };
            await userManager.CreateAsync(writer2, writer2Password);
        }
        if (!await userManager.IsInRoleAsync(writer2, "Writer"))
        {
            await userManager.AddToRoleAsync(writer2, "Writer");
        }

        // Create admin account (a@a.a)
        var adminUserName = "admin";
        var adminEmail = "a@a.a";
        var adminPassword = "P@$$w0rd";

        //Seeds sample articles
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
                    ContentHtml = "<p>I hope you will enjoy your experience with Ink & Pen! Here we critique literary works, such as novels, plays, and poems. The goal is to create a space to regularly exercise critical thought, which is becoming more and more scarce in the AI-dominated era. </p>",
                    AuthorId = admin.Id,
                },
                new Article
                {
                    Title = "Alas!",
                    ContentHtml = "<p>Just a personal thought, but I thought Brothers Karamazov had a rushed ending.</p>",
                    AuthorId = writer.Id,
                },
                new Article
                {
                    Title = "Reiteration",
                    ContentHtml = "<p>Brothers Karamazov is a great book! But, the writer thought the ending did not do justice to the rest of the story. </p>",
                    AuthorId = admin.Id,
                },
                new Article
                {
                    Title = "Is Shakespeare overrated?",
                    ContentHtml = "<p>While some people assert that Shakespeare does not deserve the praise that he garnered for the past 450 years, I believe that for his time, he was revolutionary in terms of his character depth and eloquence. Even though writers have progressively made their characters even more complex and human, Shakespeare was probably the inspiration for most, if not all, those efforts. </p>",
                    AuthorId = writer.Id,
                },
                new Article
                {
                    Title = "Getting Started",
                    ContentHtml = "<p>If you're new here, please consider becoming a writer and contributing to this growing platform. To register, please navigate to Contact. </p>",
                    AuthorId = admin.Id,
                },
                new Article
                {
                    Title = "Contact",
                    ContentHtml = "<p>My email is a@a.a. Please reach out to me if you have any interest in becoming a writer here. </p>",
                    AuthorId = admin.Id,
                }
            );
            await db.SaveChangesAsync();
        }
    }
}
