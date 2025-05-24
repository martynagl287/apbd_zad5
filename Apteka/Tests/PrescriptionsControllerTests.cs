using Xunit;
using Moq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Apteka.Controllers;
using Apteka.Models;
using Apteka.Data;
using Apteka.DTOs;

namespace Apteka.Tests
{
    public class PrescriptionsControllerTests
    {
        private DatabaseContext GetInMemoryContext()
        {
            var options = new DbContextOptionsBuilder<DatabaseContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            var context = new DatabaseContext(options);
            
            context.Doctors.Add(new Doctor
            {
                IdDoctor = 1,
                FirstName = "Test",
                LastName = "Lekarz",
                Email = "test@clinic.pl"
            });

            context.Medicaments.Add(new Medicament
            {
                IdMedicament = 1,
                Name = "TestLek",
                Description = "Na coś",
                Type = "typ"
            });

            context.SaveChanges();

            return context;
        }

        [Fact]
        public async Task AddPrescription_ReturnsOk_WhenValidRequest()
        {
            var context = GetInMemoryContext();
            var controller = new PrescriptionsController(context);

            var request = new NewPrescriptionRequest
            {
                Patient = new PatientDto
                {
                    FirstName = "Jan",
                    LastName = "Kowalski",
                    Birthdate = new DateTime(2000, 1, 1)
                },
                IdDoctor = 1,
                Date = DateTime.Today,
                DueDate = DateTime.Today.AddDays(3),
                Medicaments = new List<MedicamentDoseDto>
                {
                    new MedicamentDoseDto
                    {
                        IdMedicament = 1,
                        Dose = 2,
                        Details = "po jedzeniu"
                    }
                }
            };

            var result = await controller.AddPrescription(request);

            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task AddPrescription_ReturnsNotFound_WhenMedicamentMissing()
        {
            var context = GetInMemoryContext();
            var controller = new PrescriptionsController(context);

            var request = new NewPrescriptionRequest
            {
                Patient = new PatientDto
                {
                    FirstName = "Jan",
                    LastName = "Kowalski",
                    Birthdate = new DateTime(2000, 1, 1)
                },
                IdDoctor = 1,
                Date = DateTime.Today,
                DueDate = DateTime.Today.AddDays(3),
                Medicaments = new List<MedicamentDoseDto>
                {
                    new MedicamentDoseDto
                    {
                        IdMedicament = 999,
                        Dose = 1,
                        Details = "test"
                    }
                }
            };

            var result = await controller.AddPrescription(request);

            Assert.IsType<NotFoundObjectResult>(result);
        }

        [Fact]
        public async Task AddPrescription_ReturnsBadRequest_WhenTooManyMedicaments()
        {
            var context = GetInMemoryContext();
            var controller = new PrescriptionsController(context);

            var meds = new List<MedicamentDoseDto>();
            for (int i = 0; i < 11; i++)
            {
                meds.Add(new MedicamentDoseDto
                {
                    IdMedicament = 1,
                    Dose = 1,
                    Details = "x"
                });
            }

            var request = new NewPrescriptionRequest
            {
                Patient = new PatientDto
                {
                    FirstName = "Jan",
                    LastName = "Kowalski",
                    Birthdate = new DateTime(2000, 1, 1)
                },
                IdDoctor = 1,
                Date = DateTime.Today,
                DueDate = DateTime.Today.AddDays(1),
                Medicaments = meds
            };

            var result = await controller.AddPrescription(request);

            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task AddPrescription_ReturnsBadRequest_WhenDueDateBeforeDate()
        {
            var context = GetInMemoryContext();
            var controller = new PrescriptionsController(context);

            var request = new NewPrescriptionRequest
            {
                Patient = new PatientDto
                {
                    FirstName = "Jan",
                    LastName = "Kowalski",
                    Birthdate = new DateTime(2000, 1, 1)
                },
                IdDoctor = 1,
                Date = DateTime.Today,
                DueDate = DateTime.Today.AddDays(-1),
                Medicaments = new List<MedicamentDoseDto>
                {
                    new MedicamentDoseDto
                    {
                        IdMedicament = 1,
                        Dose = 1,
                        Details = "opis"
                    }
                }
            };

            var result = await controller.AddPrescription(request);

            Assert.IsType<BadRequestObjectResult>(result);
        }
    }
}
