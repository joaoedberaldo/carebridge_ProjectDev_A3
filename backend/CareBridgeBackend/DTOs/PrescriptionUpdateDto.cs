using CareBridgeBackend.Models;

namespace CareBridgeBackend.DTOs
{
    public class PrescriptionUpdateDto
    {
        public string? Description { get; set; } 
        public PrescriptionStatus? Status { get; set; }
    }
}
