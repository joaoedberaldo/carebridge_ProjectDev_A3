namespace CareBridgeBackend.DTOs
{
    public class MedicationCreateDto
    {
        public string Name { get; set; } = string.Empty; 
        public string Dosage { get; set; } = string.Empty; 
        public string Frequency { get; set; } = string.Empty; 
        public string? Notes { get; set; }
    }
}
