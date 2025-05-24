namespace Apteka.DTOs
{
    public class NewPrescriptionRequest
    {
        public PatientDto Patient { get; set; }
        public int IdDoctor { get; set; }
        public DateTime Date { get; set; }
        public DateTime DueDate { get; set; }
        public List<MedicamentDoseDto> Medicaments { get; set; }
    }

    public class PatientDto
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DateTime Birthdate { get; set; }
    }

    public class MedicamentDoseDto
    {
        public int IdMedicament { get; set; }
        public int Dose { get; set; }
        public string Details { get; set; }
    }
}