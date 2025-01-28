namespace CareBridgeBackend.Models
{
    public class DiagnosticTemplate
    {
        public int Id { get; set; } 
        public string Name { get; set; } 
        public string Description { get; set; } 
        public int CreatedByDoctorId { get; set; } 
        public Doctor CreatedByDoctor { get; set; }
    }
}
