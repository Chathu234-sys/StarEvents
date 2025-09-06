using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Star_Events.Data.Entities;
using Star_Events.Models;

namespace Star_Events.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }
    public DbSet<UserModel> UserProfiles { get; set; }
    public DbSet<Event> Events { get; set; }
    public DbSet<Venue> Venues { get; set; }
    public DbSet<TicketType> TicketTypes { get; set; }
    public DbSet<TicketSale> TicketSales { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Set decimal precision
        builder.Entity<Event>()
            .Property(e => e.TicketPrice)
            .HasPrecision(18, 2);

        builder.Entity<TicketType>()
            .Property(t => t.Price)
            .HasPrecision(18, 2);

        builder.Entity<TicketSale>()
            .Property(s => s.TotalAmount)
            .HasPrecision(18, 2);
    }
}
