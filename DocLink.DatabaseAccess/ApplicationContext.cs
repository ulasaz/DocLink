using DocLink.Core.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace DocLink.Data
{
    public class ApplicationContext : IdentityDbContext<Account, IdentityRole<Guid>, Guid>
    {
        public DbSet<Account> Accounts { get; set; } = null!;
        public DbSet<Specialist> Specialists { get; set; } = null!;
        public DbSet<Patient> Patients { get; set; } = null!;
        public DbSet<Offer> Offers { get; set; } = null!;
        public DbSet<Review> Reviews { get; set; } = null!;
        public DbSet<Appointment> Appointments { get; set; } = null!;
        public DbSet<DoctorSchedule> DoctorSchedules { get; set; } = null!;
        public DbSet<Message> Messages { get; set; } = null!;
        public DbSet<Log> Logs { get; set; } = null!;

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
            
            builder.Entity<Message>()
                .HasOne(m => m.Sender)
                .WithMany()
                .HasForeignKey(m => m.IdSender)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Message>()
                .HasOne(m => m.Receiver)
                .WithMany()
                .HasForeignKey(m => m.IdReceiver)
                .OnDelete(DeleteBehavior.Restrict);
            
            builder.Entity<Log>()
                .HasOne(l => l.Account)
                .WithMany()
                .HasForeignKey(l => l.IdAccount)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
