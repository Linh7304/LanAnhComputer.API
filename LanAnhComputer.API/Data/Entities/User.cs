namespace LanAnhComputer.Data.Entities
{
    public class User
    {
        public long UserId { get; set; }
        public string FullName { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string? Phone { get; set; }
        public string PasswordHash { get; set; } = null!;
        public string Role { get; set; } = "Customer";
        public string? Gender { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string? AddressLine { get; set; }
        public string? Ward { get; set; }
        public string? District { get; set; }
        public string? City { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime? LastLoginAt { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        public ICollection<Order> Orders { get; set; } = new List<Order>();
        public ICollection<ChatbotHistory> ChatbotHistories { get; set; } = new List<ChatbotHistory>();

    }
}
