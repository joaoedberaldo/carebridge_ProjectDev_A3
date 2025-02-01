namespace CareBridgeBackend.DTOs
{
    public class DoctorScheduleCreateDto
    {
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public string? Description { get; set; }
    }
}
