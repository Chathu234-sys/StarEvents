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
    public new DbSet<UserModel> Users { get; set; }
    public DbSet<Event> Events { get; set; }
    public DbSet<Venue> Venues { get; set; }
    public DbSet<TicketType> TicketTypes { get; set; }
    public DbSet<TicketSale> TicketSales { get; set; }
    public DbSet<Booking> Bookings { get; set; }
    public DbSet<BookingItem> BookingItems { get; set; }
    public DbSet<Payment> Payments { get; set; }
    public DbSet<Ticket> Tickets { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Set decimal precision

        builder.Entity<TicketType>()
            .Property(t => t.Price)
            .HasPrecision(18, 2);

        builder.Entity<TicketSale>()
            .Property(s => s.TotalAmount)
            .HasPrecision(18, 2);

        builder.Entity<Booking>()
            .Property(b => b.TotalAmount)
            .HasPrecision(18, 2);


        builder.Entity<Booking>()
            .Property(b => b.FinalAmount)
            .HasPrecision(18, 2);

        builder.Entity<Payment>()
            .Property(p => p.Amount)
            .HasPrecision(18, 2);

        // Configure Ticket relationships
        builder.Entity<Ticket>()
            .HasOne(t => t.Booking)
            .WithMany()
            .HasForeignKey(t => t.BookingId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<Ticket>()
            .HasOne(t => t.BookingItem)
            .WithMany()
            .HasForeignKey(t => t.BookingItemId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Ticket>()
            .HasOne(t => t.Event)
            .WithMany()
            .HasForeignKey(t => t.EventId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Ticket>()
            .HasOne(t => t.TicketType)
            .WithMany()
            .HasForeignKey(t => t.TicketTypeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Ticket>()
            .HasOne(t => t.Customer)
            .WithMany()
            .HasForeignKey(t => t.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);

        // Configure Payment relationships
        builder.Entity<Payment>()
            .HasOne(p => p.Booking)
            .WithMany(b => b.Payments)
            .HasForeignKey(p => p.BookingId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<Payment>()
            .HasOne(p => p.Customer)
            .WithMany()
            .HasForeignKey(p => p.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);

        // Avoid multiple cascade paths for BookingItem -> TicketType
        builder.Entity<BookingItem>()
            .HasOne(bi => bi.TicketType)
            .WithMany()
            .HasForeignKey(bi => bi.TicketTypeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<BookingItem>()
            .HasOne(bi => bi.Booking)
            .WithMany(b => b.BookingItems)
            .HasForeignKey(bi => bi.BookingId)
            .OnDelete(DeleteBehavior.Cascade);

        // Map custom user profile entity to Users table
        builder.Entity<UserModel>().ToTable("Users");
    }
}
