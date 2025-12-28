using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.ComponentModel.DataAnnotations;

namespace ProMeet.Models
{
    /// <summary>
    /// Represents a specific service offered by a professional (e.g., "Deep Tissue Massage", "Advanced C# Consultation").
    /// </summary>
    public class Service
    {
        /// <summary>
        /// Unique MongoDB Identifier.
        /// </summary>
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        /// <summary>
        /// Public-facing service ID (likely for URLs or easier reference).
        /// </summary>
        public int ServiceID { get; set; }

        /// <summary>
        /// The ID of the Professional offering this service (Links to Professional.Id).
        /// </summary>
        public string ProfessionalID { get; set; }

        /// <summary>
        /// Name of the service.
        /// </summary>
        [Required]
        public string Name { get; set; }

        /// <summary>
        /// Detailed description of what the service entails.
        /// </summary>
        public string Description { get; set; }
        
        /// <summary>
        /// Cost of the service.
        /// </summary>
        [Range(0, 1000000, ErrorMessage = "Price must be greater than or equal to 0")]
        [BsonRepresentation(BsonType.Decimal128)]
        public decimal Price { get; set; }
    }
}
