using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace CareBridgeBackend.Models
{
    public class Medication
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int PrescriptionId { get; set; }
        [ForeignKey(nameof(PrescriptionId))]
        public Prescription Prescription { get; set; }

        [Required]
        [StringLength(255)]
        public string Name { get; set; }

        [Required]
        [StringLength(50)]
        public string Dosage { get; set; }  

        [Required]
        [StringLength(50)]
        public string Frequency { get; set; }  

        [StringLength(500)]
        public string? Notes { get; set; }  
    }
}
