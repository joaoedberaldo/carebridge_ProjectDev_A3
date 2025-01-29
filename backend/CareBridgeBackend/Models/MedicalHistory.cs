using System.ComponentModel.DataAnnotations;

namespace CareBridgeBackend.Models
{
    public class MedicalHistory
    {
        public int Id { get; set; }

        [Required]
        public int PatientId { get; set; } 
        public User Patient { get; set; } 

        public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
        public ICollection<PatientDiagnostic> PatientDiagnostics { get; set; } = new List<PatientDiagnostic>();
    }
}
