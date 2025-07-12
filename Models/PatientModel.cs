namespace Diagnostic_Lab.Models
{
    public class PatientModel
    {
        public int PatientId { get; set; }

        public int Age { get; set; }

        public DateOnly Dob { get; set; }

        public string Gender { get; set; } = null!;

        public int UserId { get; set; }

        public string City { get; set; } = null!;

        public bool IsActive { get; set; }

        public string? Address { get; set; }

        public DateTime CreatedDate { get; set; }

        public DateTime? ModifiedDate { get; set; }
    }
}
