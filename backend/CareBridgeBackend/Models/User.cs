﻿using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CareBridgeBackend.Models
{
    public class User
    {
        // Generic
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string FirstName { get; set; }
        [Required]
        [StringLength(100)]
        public string LastName { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Phone]
        public string? PhoneNumber { get; set; }

        [Required]
        [StringLength(255)]
        public string Password { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime DateOfBirth { get; set; }

        [Required]
        public UserRole Role { get; set; }

        // Patient Fields
        public MedicalHistory? MedicalHistory { get; set; }

        // Doctor Fields
        [StringLength(100)]
        public string? Specialization { get; set; }

        [StringLength(100)]
        public string? LicenseNumber { get; set; }

        // Relationships
        public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
        public ICollection<PatientDiagnostic> PatientDiagnostics { get; set; } = new List<PatientDiagnostic>();

        // Doctor-Assistant Many-to-Many Relationship
        public ICollection<DoctorAssistant> DoctorsAssisted { get; set; } = new List<DoctorAssistant>();
        public ICollection<DoctorAssistant> AssistedBy { get; set; } = new List<DoctorAssistant>();
    }
}
