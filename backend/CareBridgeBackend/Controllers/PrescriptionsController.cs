using CareBridgeBackend.Data;
using CareBridgeBackend.Models;
using CareBridgeBackend.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace CareBridgeBackend.Controllers
{
    [Route("api/prescriptions")]
    [ApiController]
    [Authorize(Roles = "Doctor,Patient")]
    public class PrescriptionsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public PrescriptionsController(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Create a new prescription (Doctors only)
        /// </summary>
        [Authorize(Roles = "Doctor")]
        [HttpPost]
        public async Task<IActionResult> CreatePrescription([FromBody] PrescriptionCreateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var doctorId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            var prescription = new Prescription
            {
                PatientId = dto.PatientId,
                DoctorId = doctorId,
                AppointmentId = dto.AppointmentId,
                Date = dto.Date ?? DateTime.UtcNow,
                Description = dto.Description,
                Status = PrescriptionStatus.Active
            };

            _context.Prescriptions.Add(prescription);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetPrescription), new { id = prescription.Id }, new { Message = "Prescription created successfully.", PrescriptionId = prescription.Id });
        }

        /// <summary>
        /// Get a prescription by ID (Doctor or Patient)
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetPrescription(int id)
        {
            var prescription = await _context.Prescriptions
                .Include(p => p.Doctor)
                .Include(p => p.Patient)
                .Include(p => p.Medications)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (prescription == null)
                return NotFound(new { Message = "Prescription not found." });

            return Ok(prescription);
        }

        /// <summary>
        /// Get all prescriptions for a patient (Doctor or Patient)
        /// </summary>
        [HttpGet("patient/{patientId}")]
        public async Task<IActionResult> GetPrescriptionsForPatient(int patientId)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var userRole = User.FindFirstValue(ClaimTypes.Role);

            // Patients can only retrieve their own prescriptions
            if (userRole == "Patient" && userId != patientId)
                return Unauthorized(new { Message = "You can only view your own prescriptions." });

            var prescriptions = await _context.Prescriptions
        .Where(p => p.PatientId == patientId)
        .Include(p => p.Doctor)
        .Include(p => p.Medications)
        .ToListAsync();

            var prescriptionDtos = prescriptions.Select(p => new PrescriptionDto
            {
                Id = p.Id,
                PatientId = p.PatientId,
                DoctorId = p.DoctorId,
                DoctorName = p.Doctor != null ? $"{p.Doctor.FirstName} {p.Doctor.LastName}" : string.Empty,
                Description = p.Description,
                Date = p.Date,
                Status = p.Status,
                AppointmentId = p.AppointmentId,
                Medications = p.Medications.Select(m => new MedicationDto
                {
                    Id = m.Id,
                    Name = m.Name,
                    Dosage = m.Dosage,
                    Frequency = m.Frequency,
                    Notes = m.Notes
                }).ToList()
            }).ToList();

            return Ok(prescriptionDtos);
        }

        /// <summary>
        /// Get all prescriptions for an appointment (Doctor or Patient)
        /// </summary>
        [HttpGet("appointment/{appointmentId}")]
        public async Task<IActionResult> GetPrescriptionsForAppointment(int appointmentId)
        {
            var prescriptions = await _context.Prescriptions
        .Where(p => p.AppointmentId == appointmentId)
        .Include(p => p.Doctor)
        .Include(p => p.Patient)
        .Include(p => p.Medications)
        .ToListAsync();

            var prescriptionDtos = prescriptions.Select(p => new PrescriptionDto
            {
                Id = p.Id,
                PatientId = p.PatientId,
                DoctorId = p.DoctorId,
                DoctorName = p.Doctor != null ? $"{p.Doctor.FirstName} {p.Doctor.LastName}" : string.Empty,
                PatientName = p.Patient != null ? $"{p.Patient.FirstName} {p.Patient.LastName}" : string.Empty,
                Description = p.Description,
                Date = p.Date,
                Status = p.Status,
                AppointmentId = p.AppointmentId,
                Medications = p.Medications.Select(m => new MedicationDto
                {
                    Id = m.Id,
                    Name = m.Name,
                    Dosage = m.Dosage,
                    Frequency = m.Frequency,
                    Notes = m.Notes
                }).ToList()
            }).ToList();

            return Ok(prescriptionDtos);
        }

        /// <summary>
        /// Update a prescription (Doctor only)
        /// </summary>
        [Authorize(Roles = "Doctor")]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdatePrescription(int id, [FromBody] PrescriptionUpdateDto dto)
        {
            var prescription = await _context.Prescriptions.FindAsync(id);
            if (prescription == null)
                return NotFound(new { Message = "Prescription not found." });

            var doctorId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            // Only the doctor who created the prescription can update it
            if (prescription.DoctorId != doctorId)
                return Unauthorized(new { Message = "You are not authorized to update this prescription." });

            prescription.Description = dto.Description ?? prescription.Description;
            prescription.Status = dto.Status ?? prescription.Status;

            await _context.SaveChangesAsync();
            return Ok(new { Message = "Prescription updated successfully." });
        }

        /// <summary>
        /// Delete a prescription (Doctor only)
        /// </summary>
        [Authorize(Roles = "Doctor")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePrescription(int id)
        {
            var prescription = await _context.Prescriptions.FindAsync(id);
            if (prescription == null)
                return NotFound(new { Message = "Prescription not found." });

            var doctorId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            // Only the doctor who created the prescription can delete it
            if (prescription.DoctorId != doctorId)
                return Unauthorized(new { Message = "You are not authorized to delete this prescription." });

            _context.Prescriptions.Remove(prescription);
            await _context.SaveChangesAsync();
            return Ok(new { Message = "Prescription deleted successfully." });
        }

        // Medication related

        /// <summary>
        /// Add a medication to a prescription (Doctor only)
        /// </summary>
        [Authorize(Roles = "Doctor")]
        [HttpPost("{prescriptionId}/medications")]
        public async Task<IActionResult> AddMedication(int prescriptionId, [FromBody] MedicationCreateDto dto)
        {
            var prescription = await _context.Prescriptions.FindAsync(prescriptionId);
            if (prescription == null)
                return NotFound(new { Message = "Prescription not found." });

            var doctorId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            if (prescription.DoctorId != doctorId)
                return Unauthorized(new { Message = "You are not authorized to modify this prescription." });

            var medication = new Medication
            {
                PrescriptionId = prescriptionId,
                Name = dto.Name,
                Dosage = dto.Dosage,
                Frequency = dto.Frequency,
                Notes = dto.Notes
            };

            _context.Medications.Add(medication);
            await _context.SaveChangesAsync();

            return Ok(new { Message = "Medication added successfully." });
        }

        /// <summary>
        /// Delete a medication from a prescription (Doctor only)
        /// </summary>
        [Authorize(Roles = "Doctor")]
        [HttpDelete("medications/{id}")]
        public async Task<IActionResult> DeleteMedication(int id)
        {
            var medication = await _context.Medications.FindAsync(id);
            if (medication == null)
                return NotFound(new { Message = "Medication not found." });

            _context.Medications.Remove(medication);
            await _context.SaveChangesAsync();
            return Ok(new { Message = "Medication deleted successfully." });
        }
    }
}
