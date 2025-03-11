using CareBridgeBackend.Models;
using System.ComponentModel.DataAnnotations;

namespace CareBridgeBackend.DTOs
{
    public class UserRegisterDto
    {
        [Required]
        public string FirstName { get; set; }

        [Required]
        public string LastName { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [MinLength(6)]
        public string Password { get; set; }

        [Required]
        public UserRole Role { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime DateOfBirth { get; set; }

        // Optional fields
        public string? PhoneNumber { get; set; }
        public string? Specialization { get; set; }
        public string? LicenseNumber { get; set; }
        public int? OfficeId { get; set; }
    }
}
