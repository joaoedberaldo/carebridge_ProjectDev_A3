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
            // Many-to-many relationship between Doctors and Assistants
            modelBuilder.Entity<Doctor>()
                .HasMany(d => d.Assistants)
                .WithMany(a => a.Doctors)
                .UsingEntity(j => j.ToTable("DoctorAssistant"));

            // Patient to MedicalHistory relationship (1-to-1)
            modelBuilder.Entity<Patient>()
                .HasOne(p => p.MedicalHistory)
                .WithOne(mh => mh.Patient)
                .HasForeignKey<MedicalHistory>(mh => mh.PatientId);

            // PatientDiagnostic relationships
            modelBuilder.Entity<PatientDiagnostic>()
                .HasOne(pd => pd.DiagnosticTemplate)
                .WithMany()
                .HasForeignKey(pd => pd.DiagnosticTemplateId)
                .OnDelete(DeleteBehavior.Restrict);

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

            // Seed Data
            modelBuilder.Entity<Doctor>().HasData(
                new Doctor
                {
                    Id = 1,
                    Name = "Dr. Alice Smith",
                    Email = "alice.smith@hospital.com",
                    PhoneNumber = "123-456-7890",
                    Password = "hashed_password_1",
                    Specialization = "Cardiology",
                    LicenseNumber = "CARD-001"
                },
                new Doctor
                {
                    Id = 2,
                    Name = "Dr. John Doe",
                    Email = "john.doe@hospital.com",
                    PhoneNumber = "321-654-0987",
                    Password = "hashed_password_2",
                    Specialization = "Neurology",
                    LicenseNumber = "NEUR-002"
                }
            );

            modelBuilder.Entity<Assistant>().HasData(
                new Assistant
                {
                    Id = 3,
                    Name = "Assistant Mark Green",
                    Email = "mark.green@hospital.com",
                    PhoneNumber = "234-567-8910",
                    Password = "hashed_password_3"
                },
                new Assistant
                {
                    Id = 4,
                    Name = "Assistant Jane White",
                    Email = "jane.white@hospital.com",
                    PhoneNumber = "567-890-1234",
                    Password = "hashed_password_4"
                }
            );

            modelBuilder.Entity<Patient>().HasData(
                new Patient
                {
                    Id = 5,
                    Name = "Patient Tom Brown",
                    Email = "tom.brown@health.com",
                    PhoneNumber = "456-789-0123",
                    Password = "hashed_password_5",
                    DateOfBirth = new DateTime(1985, 6, 15)
                },
                new Patient
                {
                    Id = 6,
                    Name = "Patient Lisa Black",
                    Email = "lisa.black@health.com",
                    PhoneNumber = "678-901-2345",
                    Password = "hashed_password_6",
                    DateOfBirth = new DateTime(1992, 11, 25)
                }
            );

            modelBuilder.Entity<DiagnosticTemplate>().HasData(
                new DiagnosticTemplate
                {
                    Id = 1,
                    Name = "Basic Heart Checkup",
                    Description = "Heart-related diagnostics including ECG and blood pressure",
                    CreatedByDoctorId = 1
                },
                new DiagnosticTemplate
                {
                    Id = 2,
                    Name = "Neurological Assessment",
                    Description = "Tests for neurological reflexes and cognitive functions",
                    CreatedByDoctorId = 2
                }
            );

            base.OnModelCreating(modelBuilder);
        }
    }
}
