namespace CareBridgeBackend.DTOs
{
    public class PrescriptionCreateDto
    {
        public int PatientId { get; set; } 
        public int? AppointmentId { get; set; } 
        public string Description { get; set; } = string.Empty; 
        public DateTime? Date { get; set; } 
    }
}
