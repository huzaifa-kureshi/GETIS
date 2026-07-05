using GETIS.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace GETIS.Api.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Traveler> Travelers => Set<Traveler>();
    public DbSet<Package> Packages => Set<Package>();
    public DbSet<Booking> Bookings => Set<Booking>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Traveler>()
            .HasIndex(t => t.Email)
            .IsUnique();

        modelBuilder.Entity<Booking>()
            .HasOne(b => b.Traveler)
            .WithMany(t => t.Bookings)
            .HasForeignKey(b => b.TravelerId);

        modelBuilder.Entity<Booking>()
            .HasOne(b => b.Package)
            .WithMany(p => p.Bookings)
            .HasForeignKey(b => b.PackageId);

        base.OnModelCreating(modelBuilder);
    }
}
