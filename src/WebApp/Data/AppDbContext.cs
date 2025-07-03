using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using WebApp.Models;

namespace WebApp.Data;

public class AppDbContext : IdentityDbContext<AppUser>
{
    public DbSet<TicketModel> Tickets { get; set; }
    public DbSet<ProjectModel> Projects { get; set; }
    public DbSet<TicketFile> TicketFiles { get; set; }
    public DbSet<TicketHistoryModel> TicketHistories { get; set; }
    public DbSet<Message> Messages { get; set; }
    public AppDbContext(DbContextOptions<AppDbContext> options)
    : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<TicketModel>()
            .HasOne(t => t.Project)
            .WithMany(p => p.Tickets)
            .HasForeignKey(t => t.ProjectId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<TicketModel>()
            .HasOne(t => t.CreatorUser)
            .WithMany(u => u.Tickets)
            .HasForeignKey(t => t.CreatorUserId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<TicketModel>()
            .HasOne(t => t.AssignedUser)
            .WithMany()
            .HasForeignKey(t => t.AssignedUserId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<TicketModel>()
            .HasMany(t => t.Files)
            .WithOne(f => f.Ticket)
            .HasForeignKey(t => t.TicketId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<TicketModel>()
            .Property(t => t.Status)
            .HasConversion<string>();

        modelBuilder.Entity<TicketHistoryModel>()
            .HasOne(h => h.Ticket)
            .WithMany(t => t.History)
            .HasForeignKey(h => h.TicketId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<TicketHistoryModel>()
            .Property(h => h.PropertyName)
            .HasConversion<string>();

        modelBuilder.Entity<AppUser>()
            .Property(u => u.UserTheme)
            .HasDefaultValue("standard");

        modelBuilder.Entity<Message>()
            .HasOne(m => m.Receiver)
            .WithMany(u => u.ReceivedMessages)
            .HasForeignKey(m => m.ReceiverId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Message>()
            .HasOne(m => m.Sender)
            .WithMany(u => u.SentMessages)
            .HasForeignKey(m => m.SenderId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}