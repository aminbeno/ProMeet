using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace ProMeet.Models
{
    public class Availability
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }
        
        [BsonElement("availabilityId")]
        public int AvailabilityID { get; set; }
        
        [BsonElement("professionalId")]
        public int ProfessionalID { get; set; }
        
        [BsonElement("dayOfWeek")]
        public int DayOfWeek { get; set; }
        
        [BsonElement("startTime")]
        public TimeSpan StartTime { get; set; }
        
        [BsonElement("endTime")]
        public TimeSpan EndTime { get; set; }
        
        [BsonElement("isAvailable")]
        public bool IsAvailable { get; set; } = true;
        
        // Backward compatibility properties
        [BsonIgnore]
        public TimeSpan EndHour { get => EndTime; set => EndTime = value; }
        
        [BsonIgnore]
        public TimeSpan StartHour { get => StartTime; set => StartTime = value; }
        
        [BsonIgnore]
        public bool IsRestDay { get => !IsAvailable; set => IsAvailable = !value; }
        
        [BsonElement("createdAt")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        [BsonElement("updatedAt")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        
        // Nested document for related data
        [BsonElement("professional")]
        public Professional? Professional { get; set; }
    }
}
