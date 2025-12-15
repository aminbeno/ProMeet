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
        public string ClientID { get; set; }
        
        [BsonElement("professionalId")]
        public string ProfessionalID { get; set; }

        [BsonElement("serviceId")]
        public string? ServiceID { get; set; }

        [BsonElement("serviceName")]
        public string? ServiceName { get; set; }
        
        [BsonElement("date")]
        public DateTime Date { get; set; }
        
        [BsonElement("startTime")]
        public TimeSpan StartTime { get; set; }
        
        [BsonElement("endTime")]
        public TimeSpan EndTime { get; set; }

        [BsonElement("price")]
        public double Price { get; set; }
        
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
        public ApplicationUser? Client { get; set; }
        
        [BsonElement("professional")]
        public Professional? Professional { get; set; }
        
        [BsonElement("review")]
        public Review? Review { get; set; }

        [BsonElement("suggestedDate")]
        public DateTime? SuggestedDate { get; set; }

        [BsonElement("suggestedStartTime")]
        public TimeSpan? SuggestedStartTime { get; set; }

        [BsonElement("isRescheduleRequested")]
        public bool IsRescheduleRequested { get; set; }
    }
    
    public enum AppointmentStatus
    {
        Pending,
        Confirmed,
        Completed,
        Canceled
    }
}