using WebApp.Models;

namespace WebApp.Data;

public static class DbInitializer
{
    public static void SeedDb(IApplicationBuilder app)
    {
        var context = app
            .ApplicationServices
            .CreateScope()
            .ServiceProvider
            .GetRequiredService<AppDbContext>();

        if (!context.Projects.Any())
        {
            var project1 = new ProjectModel
            {
                Title = "Project 1",
                Description = "Project 1 description",
                StartDate = new DateTime(2025, 6, 22, 0, 0, 0, DateTimeKind.Utc),
                EndDate = new DateTime(2026, 6, 10, 0, 0, 0, DateTimeKind.Utc),
            };
            context.Projects.AddRange(project1);
            context.SaveChanges();
        }

        if (!context.Tickets.Any())
        {
            var project1 = context.Projects.FirstOrDefault(p => p.Title == "Project 1");
            var creatorUser = context.Users.FirstOrDefault(u => u.UserName == "tester");

            if (project1 is null || creatorUser is null)
                throw new Exception("Seed-Daten fehlen: Projekt 'Project 1' oder User 'tester' nicht gefunden.");

            var ticket1 = new TicketModel
            {
                Title = "Ticket 1",
                Description = "Ticket 1 description",
                CreatedAt = new DateTime(2025, 6, 23, 0, 0, 0, DateTimeKind.Utc),
                AssignedAt = new DateTime(2026, 6, 23, 0, 0, 0, DateTimeKind.Utc),
                ProjectId = project1.Id,
                Project = project1,
                CreatorUser = creatorUser,
                CreatorUserId = creatorUser.Id,
            };
            context.Tickets.AddRange(ticket1);
            context.SaveChanges();
        }
    }
}