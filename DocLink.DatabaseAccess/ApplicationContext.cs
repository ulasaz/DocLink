using DocLink.Core.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace DocLink.Data;

public class ApplicationContext : IdentityDbContext<Account, IdentityRole<Guid>, Guid>
{
    public DbSet<Account> Accounts { get; set; }
    public DbSet<Specialist> Specialists { get; set; }
    public DbSet<Patient> Patients { get; set; }
    public DbSet<Offer> Offers { get; set; }
    public DbSet<Review> Reviews { get; set; }

    public ApplicationContext(DbContextOptions<ApplicationContext> options)
        : base(options) { }
    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        
        builder.Entity<Specialist>().ToTable("Specialists");
        builder.Entity<Patient>().ToTable("Patients");

        builder.Entity<Patient>()
            .HasMany(p => p.Reviews)
            .WithOne(r => r.Patient)
            .HasForeignKey(r => r.PatientId);
        
        builder.Entity<Patient>()
            .HasMany(p => p.Appointments)
            .WithOne(a => a.Patient)
            .HasForeignKey(a => a.PatientId);
        
        builder.Entity<Specialist>()
            .HasMany(s => s.Appointments)
            .WithOne(a => a.Specialist)
            .HasForeignKey(a => a.SpecialistId);
        
        builder.Entity<Specialist>()
            .HasMany(s => s.Reviews)
            .WithOne(r => r.Specialist)
            .HasForeignKey(r => r.SpecialistId);
        
        builder.Entity<Specialist>()
            .HasMany(s => s.Offers)
            .WithOne(o => o.Specialist)
            .HasForeignKey(o => o.SpecialistId);
    }
    
}