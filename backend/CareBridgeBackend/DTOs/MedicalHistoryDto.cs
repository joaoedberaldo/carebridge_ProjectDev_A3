namespace CareBridgeBackend.DTOs
{
    public class MedicalHistoryDto
    {
        public int Id { get; set; }
        public int PatientId { get; set; }
        public List<AppointmentDto> Appointments { get; set; } = new List<AppointmentDto>();
        public List<PatientDiagnosticDto> PatientDiagnostics { get; set; } = new List<PatientDiagnosticDto>();
    }
}
