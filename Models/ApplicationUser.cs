using AspNetCore.Identity.MongoDbCore.Models;
using MongoDbGenericRepository.Attributes;

namespace ProMeet.Models
{
    [CollectionName("Users")]
    public class ApplicationUser : MongoIdentityUser<Guid>
    {
        public string Name { get; set; } = string.Empty;
        public string? Phone { get; set; }
        public string? City { get; set; }
        public string? Country { get; set; }
        public string? OrganizationName { get; set; }
        public DateTime? Birthday { get; set; }
        public string? PhotoURL { get; set; }
        public string UserType { get; set; } = "Client"; // "Client" or "Professional"
        public string? ProfessionType { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        
        // Navigation properties
                public DateTime DateJoined { get; set; }
        public string? Specialization { get; set; }
        public int? Experience { get; set; }
        public string? Bio { get; set; }
        public string? ProfessionalId { get; set; }
    }

    [CollectionName("Roles")]
    public class ApplicationRole : MongoIdentityRole<Guid>
    {
    }
}

