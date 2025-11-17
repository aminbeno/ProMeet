using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace ProMeet.Models
{
    public class Review
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }
        
        [BsonElement("reviewId")]
        public int ReviewID { get; set; }
        
        [BsonElement("appointmentId")]
        public int AppointmentID { get; set; }
        
        [BsonElement("clientId")]
        public int ClientID { get; set; }
        
        [BsonElement("professionalId")]
        public int ProfessionalID { get; set; }
        
        [BsonElement("rating")]
        public int Rating { get; set; }
        
        [BsonElement("comment")]
        public string? Comment { get; set; }
        
        [BsonElement("createdAt")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        [BsonElement("updatedAt")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        
        [BsonElement("dateProvided")]
        public DateTime DateProvided { get; set; } = DateTime.UtcNow;
        
        // Nested documents for related data
        [BsonElement("appointment")]
        public Appointment? Appointment { get; set; }
        
        [BsonElement("client")]
        public User? Client { get; set; }
        
        [BsonElement("professional")]
        public Professional? Professional { get; set; }
    }
}
