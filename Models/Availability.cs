using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;

namespace ProMeet.Models
{
    /// <summary>
    /// Represents a time slot of availability (or unavailability) for a Professional.
    /// Can represent a recurring weekly schedule or a specific date override.
    /// </summary>
    public class Availability
    {
        /// <summary>
        /// Unique MongoDB Identifier.
        /// </summary>
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }
        
        /// <summary>
        /// Legacy Integer ID.
        /// </summary>
        [BsonElement("availabilityId")]
        public int AvailabilityID { get; set; }
        
        /// <summary>
        /// ID of the Professional (Professional.Id).
        /// </summary>
        [BsonElement("professionalId")]
        public string ProfessionalID { get; set; }
        
        /// <summary>
        /// Day of the week (0 = Sunday, 1 = Monday, etc.) for recurring schedules.
        /// </summary>
        [BsonElement("dayOfWeek")]
        public int DayOfWeek { get; set; }

        /// <summary>
        /// Specific date for one-off availability/unavailability. If null, applies to DayOfWeek recurringly.
        /// </summary>
        [BsonElement("date")]
        public DateTime? Date { get; set; }
        
        /// <summary>
        /// Start time of the slot.
        /// </summary>
        [BsonElement("startTime")]
        public TimeSpan StartTime { get; set; }
        
        /// <summary>
        /// End time of the slot.
        /// </summary>
        [BsonElement("endTime")]
        public TimeSpan EndTime { get; set; }
        
        /// <summary>
        /// Indicates if the professional is available during this slot. 
        /// False usually implies a "break" or "off day" override.
        /// </summary>
        [BsonElement("isAvailable")]
        public bool IsAvailable { get; set; } = true;
        
        // Backward compatibility properties
        [BsonIgnore]
        public TimeSpan EndHour { get => EndTime; set => EndTime = value; }
        
        [BsonIgnore]
        public TimeSpan StartHour { get => StartTime; set => StartTime = value; }
        
        [BsonIgnore]
        public bool IsRestDay { get => !IsAvailable; set => IsAvailable = !value; }
        
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
        
        // Nested document for related data
        [BsonElement("professional")]
        public Professional? Professional { get; set; }
    }
}