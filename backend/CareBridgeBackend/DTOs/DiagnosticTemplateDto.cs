namespace CareBridgeBackend.DTOs
{
    public class DiagnosticTemplateDto
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public int CreatedByDoctorId { get; set; }

        public string CreatedByDoctorName { get; set; }
    }
}
