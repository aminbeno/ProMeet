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
        public string ClientID { get; set; }
        
        [BsonElement("professionalId")]
        public string ProfessionalID { get; set; }
        
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

        [BsonIgnore]
        public ApplicationUser? Client { get; set; }
    }
}