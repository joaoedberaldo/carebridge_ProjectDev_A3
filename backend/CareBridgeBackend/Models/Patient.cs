namespace CareBridgeBackend.Models
{
    public class Patient
    {
        public DateTime DateOfBirth { get; set; } 
        public MedicalHistory MedicalHistory { get; set; }
        public ICollection<Appointment> Appointments { get; set; }

        public ICollection<PatientDiagnostic> PatientDiagnostics { get; set; }
    }
}
