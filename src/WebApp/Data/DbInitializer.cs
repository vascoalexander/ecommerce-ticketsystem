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
        var longDescription = """
                              Lorem ipsum dolor sit amet, consectetur adipiscing elit. Integer ante tortor,
                               ornare sit amet bibendum vel, laoreet id leo. Donec vulputate tellus in 
                               lobortis ornare. Praesent nec mattis felis. Praesent sollicitudin placerat 
                               velit nec bibendum. Sed a massa diam. Integer metus nibh, 
                               vehicula et efficitur vitae, commodo vitae elit. Nulla id lobortis enim. 
                               Morbi hendrerit condimentum enim, vel suscipit est consectetur in. Fusce 
                               vitae nunc ligula. Sed maximus condimentum posuere. Nunc porta vehicula elit. Nunc luctus lobortis magna vitae interdum. Mauris varius arcu non augue venenatis, vel tempor urna facilisis. Morbi vel lorem id risus ornare vulputate.
                              """;

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
                    Title = $"{GetRandomTicketTitle(random)}",
                    Description = $"This is ticket number {i} with randomly generated content. {longDescription}",
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

            context.Tickets.AddRange(tickets);
            context.SaveChanges();
        }

        if (!context.Messages.Any())
        {
            var users = context.Users.ToList();
            var messages = new List<Message>();

            for (int i = 0; i < 10; i++)
            {
                var sentDate = DateTime.UtcNow.AddDays(random.Next(-365, 0));
                AppUser sender;
                AppUser receiver;
                do
                {
                    sender =  users[random.Next(users.Count)];
                    receiver =  users[random.Next(users.Count)];
                } while (sender == receiver);

                var message = new Message
                {
                    Sender = sender,
                    SenderId = sender.Id,
                    Receiver = receiver,
                    ReceiverId = receiver.Id,
                    SentAt = sentDate,
                    Subject = GetRandomTicketTitle(random),
                    Body = longDescription,
                };
                messages.Add(message);
            }
            context.Messages.AddRange(messages);
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