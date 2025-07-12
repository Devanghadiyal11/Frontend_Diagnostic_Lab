namespace Diagnostic_Lab.Models
{
    public class UserModel
    {
        public int UserId { get; set; }

        public string Email { get; set; } = null!;

        public string Password { get; set; } = null!;

        public string Role { get; set; } = null!;

        public bool IsActive { get; set; }

        public string Name { get; set; } = null!;

        public string? MobileNo { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}
