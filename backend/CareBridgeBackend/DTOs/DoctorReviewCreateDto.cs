using System.ComponentModel.DataAnnotations;

namespace CareBridgeBackend.DTOs
{
    public class DoctorReviewCreateDto
    {
        [Required]
        [Range(1, 5)]
        public int Rating { get; set; }

        [Required]
        [StringLength(500)]
        public string ReviewText { get; set; }
    }
}
