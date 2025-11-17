using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace ProMeet.Models
{
    public class Appointment
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }
        
        [BsonElement("appointmentId")]
        public int AppointmentID { get; set; }
        
        [BsonElement("clientId")]
        public int ClientID { get; set; }
        
        [BsonElement("professionalId")]
        public int ProfessionalID { get; set; }
        
        [BsonElement("date")]
        public DateTime Date { get; set; }
        
        [BsonElement("startTime")]
        public TimeSpan StartTime { get; set; }
        
        [BsonElement("endTime")]
        public TimeSpan EndTime { get; set; }
        
        [BsonElement("status")]
        public AppointmentStatus Status { get; set; } = AppointmentStatus.Pending;
        
        [BsonElement("reasonForVisit")]
        public string? ReasonForVisit { get; set; }
        
        [BsonElement("notified")]
        public bool Notified { get; set; }
        
        [BsonElement("createdAt")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        [BsonElement("updatedAt")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        
        // Nested documents for related data
        [BsonElement("client")]
        public User? Client { get; set; }
        
        [BsonElement("professional")]
        public Professional? Professional { get; set; }
        
        [BsonElement("review")]
        public Review? Review { get; set; }
    }
    
    public enum AppointmentStatus
    {
        Pending,
        Confirmed,
        Completed,
        Canceled
    }
}
