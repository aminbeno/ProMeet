using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace ProMeet.Models
{
    public class Professional
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }
        
        public int ProfessionalID { get; set; }
        

        
        public int CategoryID { get; set; }
        
        [BsonElement("jobTitle")]
        public string JobTitle { get; set; } = "";
        
        [BsonElement("specialty")]
        public string Specialty { get; set; } = "";
        
        [BsonElement("bio")]
        public string Bio { get; set; } = "";
        
        [BsonElement("experience")]
        public string Experience { get; set; } = "";
        
        [BsonElement("degrees")]
        public string Degrees { get; set; } = "";
        
        [BsonElement("consultationType")]
        public string ConsultationType { get; set; } = "";
        
        [BsonElement("price")]
        public double Price { get; set; }
        
        [BsonElement("isValidated")]
        public bool IsValidated { get; set; }
        
        [BsonElement("rating")]
        public float Rating { get; set; }
        
        [BsonElement("profileActive")]
        public bool ProfileActive { get; set; }
        
        [BsonElement("createdAt")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        [BsonElement("updatedAt")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        
        // Nested documents instead of navigation properties
        [BsonElement("user")]
        public ApplicationUser? User { get; set; }
        
        [BsonElement("category")]
        public Category? Category { get; set; }
        
        // Navigation properties (not stored in MongoDB, for application use)
        [BsonIgnore]
        public virtual ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
        
        [BsonIgnore]
        public virtual ICollection<Review> Reviews { get; set; } = new List<Review>();
        
        [BsonIgnore]
        public virtual ICollection<Availability> Availabilities { get; set; } = new List<Availability>();

        [BsonIgnore]
        public virtual ICollection<Service> Services { get; set; } = new List<Service>();
    }
}

