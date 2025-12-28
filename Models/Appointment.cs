using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;

namespace ProMeet.Models
{
    /// <summary>
    /// Represents a booking between a Client and a Professional.
    /// </summary>
    public class Appointment
    {
        /// <summary>
        /// Unique MongoDB Identifier.
        /// </summary>
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }
        
        /// <summary>
        /// Public-facing Appointment ID.
        /// </summary>
        [BsonElement("appointmentId")]
        public int AppointmentID { get; set; }
        
        /// <summary>
        /// ID of the Client (ApplicationUser.Id).
        /// </summary>
        [BsonElement("clientId")]
        public string ClientID { get; set; }
        
        /// <summary>
        /// ID of the Professional (Professional.Id).
        /// </summary>
        [BsonElement("professionalId")]
        public string ProfessionalID { get; set; }

        /// <summary>
        /// Optional: ID of the specific service booked (if not a general consultation).
        /// </summary>
        [BsonElement("serviceId")]
        public string? ServiceID { get; set; }

        /// <summary>
        /// Name of the service at the time of booking (preserved even if service is renamed).
        /// </summary>
        [BsonElement("serviceName")]
        public string? ServiceName { get; set; }
        
        /// <summary>
        /// Date of the appointment.
        /// </summary>
        [BsonElement("date")]
        public DateTime Date { get; set; }
        
        /// <summary>
        /// Start time of the appointment.
        /// </summary>
        [BsonElement("startTime")]
        public TimeSpan StartTime { get; set; }
        
        /// <summary>
        /// End time of the appointment.
        /// </summary>
        [BsonElement("endTime")]
        public TimeSpan EndTime { get; set; }

        /// <summary>
        /// Agreed price for the appointment.
        /// </summary>
        [BsonElement("price")]
        [BsonRepresentation(BsonType.Decimal128)]
        public decimal Price { get; set; }
        
        /// <summary>
        /// Current status of the appointment (Pending, Confirmed, etc.).
        /// </summary>
        [BsonElement("status")]
        public AppointmentStatus Status { get; set; } = AppointmentStatus.Pending;
        
        /// <summary>
        /// Client's note or reason for the visit.
        /// </summary>
        [BsonElement("reasonForVisit")]
        public string? ReasonForVisit { get; set; }
        
        /// <summary>
        /// Flag indicating if the user has been notified (for reminders).
        /// </summary>
        [BsonElement("notified")]
        public bool Notified { get; set; }
        
        /// <summary>
        /// Timestamp of creation.
        /// </summary>
        [BsonElement("createdAt")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        /// <summary>
        /// Timestamp of last update.
        /// </summary>
        [BsonElement("updatedAt")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        
        // Nested documents/Navigation properties for related data (not always stored, populated on demand)
        
        [BsonElement("client")]
        public ApplicationUser? Client { get; set; }
        
        [BsonElement("professional")]
        public Professional? Professional { get; set; }
        
        [BsonElement("review")]
        public Review? Review { get; set; }

        /// <summary>
        /// Proposed new date if rescheduling is requested.
        /// </summary>
        [BsonElement("suggestedDate")]
        public DateTime? SuggestedDate { get; set; }

        /// <summary>
        /// Proposed new start time if rescheduling is requested.
        /// </summary>
        [BsonElement("suggestedStartTime")]
        public TimeSpan? SuggestedStartTime { get; set; }

        /// <summary>
        /// Flag indicating if a reschedule has been requested by either party.
        /// </summary>
        [BsonElement("isRescheduleRequested")]
        public bool IsRescheduleRequested { get; set; }
    }
    
    /// <summary>
    /// Lifecycle states of an Appointment.
    /// </summary>
    public enum AppointmentStatus
    {
        Pending,
        Confirmed,
        Completed,
        Canceled
    }
}
