using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;

namespace ProMeet.Models
{
    /// <summary>
    /// Represents a review left by a Client for a Professional after an Appointment.
    /// </summary>
    public class Review
    {
        /// <summary>
        /// Unique MongoDB Identifier.
        /// </summary>
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }
        
        /// <summary>
        /// Public-facing Review ID.
        /// </summary>
        [BsonElement("reviewId")]
        public int ReviewID { get; set; }
        
        /// <summary>
        /// ID of the Appointment being reviewed (Links to Appointment.AppointmentID or Id - check usage).
        /// Note: Usage implies link to AppointmentID (int) or Id (string). To be verified.
        /// </summary>
        [BsonElement("appointmentId")]
        public int AppointmentID { get; set; }
        
        /// <summary>
        /// ID of the Client leaving the review (ApplicationUser.Id).
        /// </summary>
        [BsonElement("clientId")]
        public string ClientID { get; set; }
        
        /// <summary>
        /// ID of the Professional being reviewed (Professional.Id).
        /// </summary>
        [BsonElement("professionalId")]
        public string ProfessionalID { get; set; }
        
        /// <summary>
        /// Rating value (e.g., 1-5 stars).
        /// </summary>
        [BsonElement("rating")]
        public int Rating { get; set; }
        
        /// <summary>
        /// Textual comment for the review.
        /// </summary>
        [BsonElement("comment")]
        public string? Comment { get; set; }
        
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
        
        /// <summary>
        /// Date the review was provided (can be same as CreatedAt).
        /// </summary>
        [BsonElement("dateProvided")]
        public DateTime DateProvided { get; set; } = DateTime.UtcNow;

        // Navigation properties (not stored in MongoDB)
        
        [BsonIgnore]
        public ApplicationUser? Client { get; set; }

        [BsonIgnore]
        public Professional? Professional { get; set; }
    }
}
