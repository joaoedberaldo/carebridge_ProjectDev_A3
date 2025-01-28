namespace CareBridgeBackend.Models
{
    public class MedicalHistory
    {
        public int Id { get; set; } 
        public int PatientId { get; set; } 
        public Patient Patient { get; set; } 

        public ICollection<Appointment> Appointments { get; set; } 
        public ICollection<PatientDiagnostic> PatientDiagnostics { get; set; }
    }
}
