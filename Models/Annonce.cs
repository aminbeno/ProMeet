using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.ComponentModel.DataAnnotations;

namespace ProMeet.Models
{
    /// <summary>
    /// Represents an advertisement or listing created by a professional.
    /// May be used for simplified listing creation (Legacy/Alternative to Service).
    /// </summary>
    public class Annonce
    {
        /// <summary>
        /// Unique MongoDB Identifier.
        /// </summary>
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        /// <summary>
        /// ID of the Professional creating the listing.
        /// </summary>
        public string ProfessionalId { get; set; }

        /// <summary>
        /// Title of the listing.
        /// </summary>
        [Required]
        public string Title { get; set; }

        /// <summary>
        /// Category of the service (e.g., "Plumbing").
        /// </summary>
        [Required]
        public string Category { get; set; }

        /// <summary>
        /// Specific specialty (e.g., "Leak Repair").
        /// </summary>
        [Required]
        public string Specialty { get; set; }

        /// <summary>
        /// Detailed description.
        /// </summary>
        [Required]
        public string Description { get; set; }

        /// <summary>
        /// Years of experience related to this listing.
        /// </summary>
        [Required]
        [Range(0, 50)]
        public int YearsOfExperience { get; set; }

        /// <summary>
        /// Location where the service is provided.
        /// </summary>
        public string Location { get; set; }

        /// <summary>
        /// Availability description (textual).
        /// </summary>
        [Required]
        public string Availability { get; set; }
    }
}