using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Apteka.Data;
using Apteka.DTOs;
using Apteka.Models;

namespace Apteka.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PrescriptionsController : ControllerBase
    {
        private readonly DatabaseContext _context;
        public PrescriptionsController(DatabaseContext context)
        {
            _context = context;
        }

        [HttpPost]
        public async Task<IActionResult> AddPrescription(NewPrescriptionRequest request)
        {
            if (request.DueDate < request.Date)
                return BadRequest("DueDate musi być większy lub równy Date.");

            if (request.Medicaments.Count > 10)
                return BadRequest("Recepta nie może zawierać więcej niż 10 leków.");

            var doctor = await _context.Doctors.FindAsync(request.IdDoctor);
            if (doctor == null)
                return NotFound("Doktor nie istnieje.");

            var patient = await _context.Patients
                .FirstOrDefaultAsync(p =>
                    p.FirstName == request.Patient.FirstName &&
                    p.LastName == request.Patient.LastName &&
                    p.Birthdate == request.Patient.Birthdate);

            if (patient == null)
            {
                patient = new Patient
                {
                    FirstName = request.Patient.FirstName,
                    LastName = request.Patient.LastName,
                    Birthdate = request.Patient.Birthdate
                };
                _context.Patients.Add(patient);
                await _context.SaveChangesAsync();
            }

            foreach (var m in request.Medicaments)
            {
                if (!await _context.Medicaments.AnyAsync(md => md.IdMedicament == m.IdMedicament))
                    return NotFound($"Lek {m.IdMedicament} nie istnieje.");
            }

            var prescription = new Prescription
            {
                Date = request.Date,
                DueDate = request.DueDate,
                IdDoctor = doctor.IdDoctor,
                IdPatient = patient.IdPatient,
                Prescription_Medicaments = request.Medicaments.Select(m => new Prescription_Medicament
                {
                    IdMedicament = m.IdMedicament,
                    Dose = m.Dose,
                    Details = m.Details
                }).ToList()
            };

            _context.Prescriptions.Add(prescription);
            await _context.SaveChangesAsync();

            return Ok("Recepta została wystawiona.");
        }

        [HttpGet("/api/patients/{id}")]
        public async Task<IActionResult> GetPatientDetails(int id)
        {
            var patient = await _context.Patients
                .Include(p => p.Prescriptions)
                    .ThenInclude(pr => pr.Doctor)
                .Include(p => p.Prescriptions)
                    .ThenInclude(pr => pr.Prescription_Medicaments)
                        .ThenInclude(pm => pm.Medicament)
                .FirstOrDefaultAsync(p => p.IdPatient == id);

            if (patient == null)
                return NotFound("Pacjent nie istnieje.");

            var result = new PatientDetailsDto
            {
                IdPatient = patient.IdPatient,
                FirstName = patient.FirstName,
                LastName = patient.LastName,
                Birthdate = patient.Birthdate,
                Prescriptions = patient.Prescriptions
                    .OrderBy(p => p.DueDate)
                    .Select(p => new PrescriptionDto
                    {
                        IdPrescription = p.IdPrescription,
                        Date = p.Date,
                        DueDate = p.DueDate,
                        Doctor = new DoctorDto
                        {
                            IdDoctor = p.Doctor.IdDoctor,
                            FirstName = p.Doctor.FirstName,
                            LastName = p.Doctor.LastName,
                            Email = p.Doctor.Email
                        },
                        Medicaments = p.Prescription_Medicaments
                            .Select(pm => new MedicamentDto
                            {
                                IdMedicament = pm.Medicament.IdMedicament,
                                Name = pm.Medicament.Name,
                                Description = pm.Medicament.Description,
                                Type = pm.Medicament.Type,
                                Dose = pm.Dose,
                                Details = pm.Details
                            }).ToList()
                    }).ToList()
            };

            return Ok(result);
        }
    }
}