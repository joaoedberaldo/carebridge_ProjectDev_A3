using System.ComponentModel.DataAnnotations;

namespace CareBridgeBackend.DTOs
{
    public class DiagnosticTemplateUpdateDto
    {
        [Required]
        [StringLength(200)]
        public string Name { get; set; }

        public string Description { get; set; }
    }
}
