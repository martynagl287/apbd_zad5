using Microsoft.EntityFrameworkCore;
using Apteka.Models;

namespace Apteka.Data
{
    public class DatabaseContext : DbContext
    {
        public DbSet<Patient> Patients { get; set; }
        public DbSet<Doctor> Doctors { get; set; }
        public DbSet<Medicament> Medicaments { get; set; }
        public DbSet<Prescription> Prescriptions { get; set; }
        public DbSet<Prescription_Medicament> Prescription_Medicaments { get; set; }

        protected DatabaseContext()
        {
        }

        public DatabaseContext(DbContextOptions options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Patient>(p =>
            {
                p.ToTable("Patient");
                p.HasKey(e => e.IdPatient);
                p.Property(e => e.FirstName).HasMaxLength(100).IsRequired();
                p.Property(e => e.LastName).HasMaxLength(100).IsRequired();
                p.Property(e => e.Birthdate).IsRequired();
            });

            modelBuilder.Entity<Doctor>(d =>
            {
                d.ToTable("Doctor");
                d.HasKey(e => e.IdDoctor);
                d.Property(e => e.FirstName).HasMaxLength(100).IsRequired();
                d.Property(e => e.LastName).HasMaxLength(100).IsRequired();
                d.Property(e => e.Email).HasMaxLength(100);
            });

            modelBuilder.Entity<Medicament>(m =>
            {
                m.ToTable("Medicament");
                m.HasKey(e => e.IdMedicament);
                m.Property(e => e.Name).HasMaxLength(100).IsRequired();
                m.Property(e => e.Description).HasMaxLength(300);
                m.Property(e => e.Type).HasMaxLength(100);
            });

            modelBuilder.Entity<Prescription>(p =>
            {
                p.ToTable("Prescription");
                p.HasKey(e => e.IdPrescription);
                p.Property(e => e.Date).IsRequired();
                p.Property(e => e.DueDate).IsRequired();

                p.HasOne(e => e.Patient)
                    .WithMany(p => p.Prescriptions)
                    .HasForeignKey(e => e.IdPatient);

                p.HasOne(e => e.Doctor)
                    .WithMany(d => d.Prescriptions)
                    .HasForeignKey(e => e.IdDoctor);
            });

            modelBuilder.Entity<Prescription_Medicament>(pm =>
            {
                pm.ToTable("Prescription_Medicament");
                pm.HasKey(e => new { e.IdMedicament, e.IdPrescription });

                pm.Property(e => e.Dose).IsRequired();
                pm.Property(e => e.Details).HasMaxLength(300);

                pm.HasOne(e => e.Medicament)
                    .WithMany(m => m.Prescription_Medicaments)
                    .HasForeignKey(e => e.IdMedicament);

                pm.HasOne(e => e.Prescription)
                    .WithMany(p => p.Prescription_Medicaments)
                    .HasForeignKey(e => e.IdPrescription);
            });
        }
    }
}
