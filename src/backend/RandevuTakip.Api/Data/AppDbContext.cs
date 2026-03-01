using Microsoft.EntityFrameworkCore;
using RandevuTakip.Api.Models;

namespace RandevuTakip.Api.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Tenant> Tenants { get; set; } = null!;
    public DbSet<Service> Services { get; set; } = null!;
    public DbSet<WorkingHours> WorkingHours { get; set; } = null!;
    public DbSet<Appointment> Appointments { get; set; } = null!;
    public DbSet<Admin> Admins { get; set; } = null!;
    public DbSet<Staff> Staff { get; set; } = null!;
    public DbSet<StaffService> StaffServices { get; set; } = null!;
    public DbSet<AuditLog> AuditLogs { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Constraints
        modelBuilder.Entity<Tenant>()
            .HasIndex(t => t.Slug)
            .IsUnique();

        modelBuilder.Entity<Service>()
            .HasOne(s => s.Tenant)
            .WithMany(t => t.Services)
            .HasForeignKey(s => s.TenantId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<StaffService>()
            .HasOne(ss => ss.Staff)
            .WithMany(s => s.StaffServices)
            .HasForeignKey(ss => ss.StaffId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<StaffService>()
            .HasOne(ss => ss.Service)
            .WithMany(s => s.StaffServices)
            .HasForeignKey(ss => ss.ServiceId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
