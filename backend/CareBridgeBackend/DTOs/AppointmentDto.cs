namespace CareBridgeBackend.DTOs
{
    public class AppointmentDto
    {
        public int Id { get; set; }
        public DateTime AppointmentDate { get; set; }
        public string? Notes { get; set; }
    }
}
