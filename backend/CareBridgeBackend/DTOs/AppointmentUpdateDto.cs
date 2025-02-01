namespace CareBridgeBackend.DTOs
{
    public class AppointmentUpdateDto
    {
        public DateTime? AppointmentDate { get; set; }
        public int DoctorId { get; set; }
        public int PatientId { get; set; }
        public string? Notes { get; set; }
    }
}
