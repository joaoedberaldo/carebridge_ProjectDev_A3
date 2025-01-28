namespace CareBridgeBackend.Models
{
    public class Appointment
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public int DoctorId { get; set; } 
        public Doctor Doctor { get; set; } 
        public int? AssistantId { get; set; } 
        public Assistant Assistant { get; set; } 
        public int PatientId { get; set; } 
        public Patient Patient { get; set; } 
        public string Notes { get; set; }
    }
}
