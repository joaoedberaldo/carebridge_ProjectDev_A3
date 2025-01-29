using System.ComponentModel.DataAnnotations;

namespace CareBridgeBackend.Models
{
    public class Doctor : User
    {
        [Required]
        [StringLength(100)]
        public string Specialization { get; set; }

        [Required]
        [StringLength(100)]
        public string LicenseNumber { get; set; }
        public ICollection<Assistant> Assistants { get; set; } = new List<Assistant>();
        public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();

        public ICollection<PatientDiagnostic> PatientDiagnostics { get; set; } = new List<PatientDiagnostic>();
    }
}
