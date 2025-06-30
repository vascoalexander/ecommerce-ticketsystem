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

        var random = new Random();
        var categories = new[] { "feature", "bug", "enhancement", "documentation", "maintenance" };
        var statuses = new[] { TicketStatus.Open, TicketStatus.Closed, TicketStatus.InProgress };

        if (!context.Projects.Any())
        {
            var projects = new List<ProjectModel>();

            for (int i = 0; i < 10; i++)
            {
                var startDate = DateTime.UtcNow.AddDays(random.Next(-365, 0));
                var endDate = startDate.AddDays(random.Next(30, 365));

                var project = new ProjectModel
                {
                    Title = $"Project {i}",
                    Description = $"Automatically generated project {i} with random data for testing purposes.",
                    Category = categories[random.Next(categories.Length)],
                    StartDate = startDate,
                    EndDate = endDate,
                };

                projects.Add(project);
            }
            context.Projects.AddRange(projects);
            context.SaveChanges();
        }

        if (!context.Tickets.Any())
        {
            var projects = context.Projects.ToList();
            var users = context.Users.ToList();

            if (!projects.Any() || !users.Any())
                throw new Exception("Seed-Daten fehlen: Keine Projekte oder User gefunden.");

            var tickets = new List<TicketModel>();

            for (int i = 0; i < 50; i++)
            {
                var randomProject = projects[random.Next(projects.Count)];
                var randomUser = users[random.Next(users.Count)];

                var createdAt = DateTime.UtcNow.AddDays(random.Next(-180, 0));
                var assignedAt = createdAt.AddDays(random.Next(0, 30));

                var ticket = new TicketModel
                {
                    Title = $"Ticket {i}: {GetRandomTicketTitle(random)}",
                    Description = $"This is ticket number {i} with randomly generated content. " +
                                  $"Priority level: {random.Next(1, 6)}. " +
                                  $"Estimated effort: {random.Next(1, 21)} hours.",
                    CreatedAt = createdAt,
                    AssignedAt = assignedAt,
                    AssignedUser = randomUser,
                    ProjectId = randomProject.Id,
                    Project = randomProject,
                    Status = statuses[random.Next(statuses.Length)],
                    CreatorUser = randomUser,
                    CreatorUserId = randomUser.Id,
                };

                tickets.Add(ticket);
            }


            // var project1 = context.Projects.FirstOrDefault(p => p.Title == "Project 1");
            // var creatorUser = context.Users.FirstOrDefault(u => u.UserName == "tester");
            //
            // if (project1 is null || creatorUser is null)
            //     throw new Exception("Seed-Daten fehlen: Projekt 'Project 1' oder User 'tester' nicht gefunden.");

            // var ticket1 = new TicketModel
            // {
            //     Title = "Ticket 1",
            //     Description = "Ticket 1 description",
            //     CreatedAt = new DateTime(2025, 6, 23, 0, 0, 0, DateTimeKind.Utc),
            //     AssignedAt = new DateTime(2026, 6, 23, 0, 0, 0, DateTimeKind.Utc),
            //     ProjectId = project1.Id,
            //     Project = project1,
            //     Status = "Open",
            //     CreatorUser = creatorUser,
            //     CreatorUserId = creatorUser.Id,
            // };
            context.Tickets.AddRange(tickets);
            context.SaveChanges();
        }
    }
    static string GetRandomTicketTitle(Random random)
    {
        var actions = new[] { "Fix", "Implement", "Update", "Refactor", "Add", "Remove", "Optimize", "Debug" };
        var subjects = new[] { "user interface", "database connection", "authentication system",
            "API endpoint", "navigation menu", "search functionality", "email notifications",
            "file upload", "data validation", "performance issue" };

        return $"{actions[random.Next(actions.Length)]} {subjects[random.Next(subjects.Length)]}";
    }
}