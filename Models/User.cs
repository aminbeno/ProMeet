using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace ProMeet.Models
{
    public class User
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }
        
        [BsonElement("userId")]
        public int UserID { get; set; }
        
        [BsonElement("name")]
        public string Name { get; set; } = "";
        
        [BsonElement("email")]
        public string Email { get; set; } = "";
        
        [BsonElement("password")]
        public string Password { get; set; } = "";
        
        [BsonElement("phone")]
        public string? Phone { get; set; }
        
        [BsonElement("photoUrl")]
        public string? PhotoURL { get; set; }
        
        [BsonElement("city")]
        public string? City { get; set; }
        
        [BsonElement("country")]
        public string? Country { get; set; }
        
        [BsonElement("userType")]
        public string UserType { get; set; } = "Client";
        
        [BsonElement("createdAt")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        [BsonElement("updatedAt")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        
        // Additional properties for professionals
        [BsonElement("specialization")]
        public string? Specialization { get; set; }
        
        [BsonElement("experience")]
        public int? Experience { get; set; }
        
        // Backward compatibility property
        [BsonIgnore]
        public DateTime DateJoined { get => CreatedAt; set => CreatedAt = value; }
        
        // Navigation properties (not stored in MongoDB, for application use)
        [BsonIgnore]
        public virtual Professional? Professional { get; set; }
        
        [BsonIgnore]
        public virtual ICollection<Appointment> ClientAppointments { get; set; } = new List<Appointment>();
        
        [BsonIgnore]
        public virtual ICollection<Chat> ClientChats { get; set; } = new List<Chat>();
    }
}
