using System.ComponentModel.DataAnnotations;

namespace CareBridgeBackend.Models
{
    public class DiagnosticTemplate
    {
        public int Id { get; set; }

        [Required]
        [StringLength(200)]
        public string Name { get; set; } 
        public string Description { get; set; } 
        public int CreatedByDoctorId { get; set; } 
        public User CreatedByDoctor { get; set; }
    }
}
