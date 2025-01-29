using System.ComponentModel.DataAnnotations;

namespace CareBridgeBackend.Models
{
    public class Treatment
    {
        public int Id { get; set; } 
        public string Name { get; set; }

        [Required]
        [StringLength(500)]
        public string Description { get; set; }

        [Required]
        public int PatientDiagnosticId { get; set; } 
        public PatientDiagnostic PatientDiagnostic { get; set; } 
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; } 
    }
}
