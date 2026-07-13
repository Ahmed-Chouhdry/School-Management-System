using Microsoft.EntityFrameworkCore;
using SchoolManagementSystem.Domain.Entities;

namespace SchoolManagementSystem.Infrastructure.Persistence;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<WalletAdjustment> WalletAdjustments => Set<WalletAdjustment>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.Entity<User>(b =>
        {
            b.HasKey(u => u.Id);
            b.Property(u => u.FirstName).IsRequired().HasMaxLength(100);
            b.Property(u => u.LastName).IsRequired().HasMaxLength(100);
            b.Property(u => u.Email).IsRequired().HasMaxLength(200);
            b.Property(u => u.WalletBalance).HasColumnType("decimal(18,2)");
            b.Property(u => u.Role).HasConversion<string>();
            b.Property(u => u.Status).HasConversion<string>();

            b.HasMany(u => u.WalletAdjustments)
             .WithOne()
             .HasForeignKey(w => w.UserId)
             .OnDelete(DeleteBehavior.Cascade);

            b.Navigation(u => u.WalletAdjustments).UsePropertyAccessMode(PropertyAccessMode.Field);
        });

        builder.Entity<WalletAdjustment>(b =>
        {
            b.HasKey(w => w.Id);
            b.Property(w => w.Amount).HasColumnType("decimal(18,2)");
            b.Property(w => w.ResultingBalance).HasColumnType("decimal(18,2)");
            b.Property(w => w.Reason).HasMaxLength(500);
        });
    }
}