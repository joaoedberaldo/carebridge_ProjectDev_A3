namespace CareBridgeBackend.DTOs
{
    public class PatientDiagnosticDto
    {
        public int Id { get; set; }
        public int DiagnosticTemplateId { get; set; }
        public DateTime DateDiagnosed { get; set; }
        public string? Notes { get; set; }
    }
}
