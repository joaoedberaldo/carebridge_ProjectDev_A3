using System.ComponentModel.DataAnnotations;

namespace CareBridgeBackend.DTOs
{
    public class TreatmentCreateDto
    {
        [Required]
        public string Name { get; set; }

        [Required]
        [StringLength(500)]
        public string Description { get; set; }

        [Required]
        public int PatientDiagnosticId { get; set; }

        [Required]
        public DateTime StartDate { get; set; }

        public DateTime? EndDate { get; set; }
    }
}
