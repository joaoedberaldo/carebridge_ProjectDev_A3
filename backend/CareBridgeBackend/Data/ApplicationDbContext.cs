using CareBridgeBackend.Models;
using Microsoft.EntityFrameworkCore;

namespace CareBridgeBackend.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        // DbSets for each entity
        public DbSet<User> Users { get; set; }
        public DbSet<Doctor> Doctors { get; set; }
        public DbSet<Assistant> Assistants { get; set; }
        public DbSet<Patient> Patients { get; set; }
        public DbSet<Appointment> Appointments { get; set; }
        public DbSet<DiagnosticTemplate> DiagnosticTemplates { get; set; }
        public DbSet<PatientDiagnostic> PatientDiagnostics { get; set; }
        public DbSet<MedicalHistory> MedicalHistories { get; set; }
        public DbSet<Treatment> Treatments { get; set; }

        // Configuring relationships
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Configure many-to-many relationship between Doctors and Assistants
            modelBuilder.Entity<Doctor>()
                .HasMany(d => d.Assistants)
                .WithMany(a => a.Doctors)
                .UsingEntity(j => j.ToTable("DoctorAssistant"));

            // Configure Patient to MedicalHistory relationship (1-to-1)
            modelBuilder.Entity<Patient>()
                .HasOne(p => p.MedicalHistory)
                .WithOne(mh => mh.Patient)
                .HasForeignKey<MedicalHistory>(mh => mh.PatientId);

            // Configure PatientDiagnostic relationships
            modelBuilder.Entity<PatientDiagnostic>()
                .HasOne(pd => pd.DiagnosticTemplate)
                .WithMany()
                .HasForeignKey(pd => pd.DiagnosticTemplateId);

            modelBuilder.Entity<PatientDiagnostic>()
                .HasOne(pd => pd.Patient)
                .WithMany(p => p.PatientDiagnostics)
                .HasForeignKey(pd => pd.PatientId);

            modelBuilder.Entity<PatientDiagnostic>()
                .HasOne(pd => pd.Doctor)
                .WithMany(d => d.PatientDiagnostics)
                .HasForeignKey(pd => pd.DoctorId);

            // Configure Treatment relationships
            modelBuilder.Entity<Treatment>()
                .HasOne(t => t.PatientDiagnostic)
                .WithMany(pd => pd.Treatments)
                .HasForeignKey(t => t.PatientDiagnosticId);

            base.OnModelCreating(modelBuilder);
        }
    }
}
