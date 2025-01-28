using System.ComponentModel.DataAnnotations;

namespace CareBridgeBackend.Models
{
    public class Patient
    {
        [Required]
        [DataType(DataType.Date)]
        public DateTime DateOfBirth { get; set; } 
        public MedicalHistory MedicalHistory { get; set; }
        public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();

        public ICollection<PatientDiagnostic> PatientDiagnostics { get; set; } = new List<PatientDiagnostic>();
    }
}
