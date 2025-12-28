using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;

namespace ProMeet.Models
{
    /// <summary>
    /// Represents a professional service provider in the system.
    /// This document stores professional-specific details and includes an embedded copy of the user's basic info.
    /// </summary>
    public class Professional
    {
        /// <summary>
        /// Unique identifier (MongoDB ObjectId).
        /// </summary>
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }
        
        /// <summary>
        /// Legacy integer ID (deprecated in favor of string Id).
        /// </summary>
        public int ProfessionalID { get; set; }
        
        /// <summary>
        /// Legacy Category ID.
        /// </summary>
        public int CategoryID { get; set; }
        
        /// <summary>
        /// Professional's job title (e.g., "Senior Developer").
        /// </summary>
        [BsonElement("jobTitle")]
        public string JobTitle { get; set; } = "";
        
        /// <summary>
        /// Primary area of expertise.
        /// </summary>
        [BsonElement("specialty")]
        public string Specialty { get; set; } = "";
        
        /// <summary>
        /// Biography or description of services.
        /// </summary>
        [BsonElement("bio")]
        public string Bio { get; set; } = "";
        
        /// <summary>
        /// Years of experience or description of experience.
        /// </summary>
        [BsonElement("experience")]
        public string Experience { get; set; } = "";
        
        /// <summary>
        /// Academic degrees or certifications.
        /// </summary>
        [BsonElement("degrees")]
        public string Degrees { get; set; } = "";
        
        /// <summary>
        /// Type of consultation offered (e.g., "Online", "In-Person").
        /// </summary>
        [BsonElement("consultationType")]
        public string ConsultationType { get; set; } = "";
        
        /// <summary>
        /// Base price for a consultation (Standardized to Decimal128 for currency precision).
        /// </summary>
        [BsonElement("price")]
        [BsonRepresentation(BsonType.Decimal128)]
        public decimal Price { get; set; }
        
        /// <summary>
        /// Indicates if the professional's credentials have been verified.
        /// </summary>
        [BsonElement("isValidated")]
        public bool IsValidated { get; set; }
        
        /// <summary>
        /// Average rating from client reviews.
        /// </summary>
        [BsonElement("rating")]
        public float Rating { get; set; }
        
        /// <summary>
        /// Indicates if the profile is visible to users.
        /// </summary>
        [BsonElement("profileActive")]
        public bool ProfileActive { get; set; }
        
        /// <summary>
        /// Timestamp of profile creation.
        /// </summary>
        [BsonElement("createdAt")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        /// <summary>
        /// Timestamp of last profile update.
        /// </summary>
        [BsonElement("updatedAt")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        
        /// <summary>
        /// Embedded User document containing basic identity info (Name, Email, etc.).
        /// Denormalized for performance to avoid frequent lookups in the Users collection.
        /// </summary>
        [BsonElement("user")]
        public ApplicationUser? User { get; set; }
        
        /// <summary>
        /// Category of the professional.
        /// </summary>
        [BsonElement("category")]
        public Category? Category { get; set; }
        
        // Navigation properties (not stored in MongoDB, populated by application logic)
        
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
