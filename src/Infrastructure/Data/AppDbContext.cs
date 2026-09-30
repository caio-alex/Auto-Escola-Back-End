using AutoEscola.Api.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AutoEscola.Api.Infrastructure.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<Student> Students => Set<Student>();
    public DbSet<Instructor> Instructors => Set<Instructor>();
    public DbSet<Vehicle> Vehicles => Set<Vehicle>();
    public DbSet<Lesson> Lessons => Set<Lesson>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(u => u.Id);
            entity.HasIndex(u => u.Email).IsUnique();
            entity.HasIndex(u => u.Cpf).IsUnique();
        });

        modelBuilder.Entity<Student>(entity =>
        {
            entity.HasKey(s => s.Id);
            entity.HasOne(s => s.User)
                  .WithOne(u => u.StudentProfile)
                  .HasForeignKey<Student>(s => s.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Instructor>(entity =>
        {
            entity.HasKey(i => i.Id);
            entity.HasOne(i => i.User)
                  .WithOne(u => u.InstructorProfile)
                  .HasForeignKey<Instructor>(i => i.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Vehicle>(entity =>
        {
            entity.HasKey(v => v.Id);
            entity.HasIndex(v => v.Plate).IsUnique();
        });

        modelBuilder.Entity<Lesson>(entity =>
        {
            entity.HasKey(l => l.Id);

            entity.HasOne(l => l.Student)
                  .WithMany(s => s.Lessons)
                  .HasForeignKey(l => l.StudentId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(l => l.Instructor)
                  .WithMany(i => i.Lessons)
                  .HasForeignKey(l => l.InstructorId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(l => l.Vehicle)
                  .WithMany(v => v.Lessons)
                  .HasForeignKey(l => l.VehicleId)
                  .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
