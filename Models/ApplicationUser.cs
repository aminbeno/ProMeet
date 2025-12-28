using AspNetCore.Identity.MongoDbCore.Models;
using MongoDbGenericRepository.Attributes;
using System;

namespace ProMeet.Models
{
    /// <summary>
    /// Represents a user of the application, extending the default MongoIdentityUser.
    /// This class stores authentication data and basic profile information common to all users (Clients and Professionals).
    /// </summary>
    [CollectionName("Users")]
    public class ApplicationUser : MongoIdentityUser<Guid>
    {
        /// <summary>
        /// Full name of the user.
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Contact phone number.
        /// </summary>
        public string? Phone { get; set; }

        /// <summary>
        /// City of residence or operation.
        /// </summary>
        public string? City { get; set; }

        /// <summary>
        /// Country of residence.
        /// </summary>
        public string? Country { get; set; }

        /// <summary>
        /// Name of the organization (if applicable, primarily for Professionals).
        /// </summary>
        public string? OrganizationName { get; set; }

        /// <summary>
        /// Date of birth.
        /// </summary>
        public DateTime? Birthday { get; set; }

        /// <summary>
        /// URL path to the user's profile picture.
        /// </summary>
        public string? PhotoURL { get; set; }

        /// <summary>
        /// Discriminator for user role: "Client" or "Professional".
        /// </summary>
        public string UserType { get; set; } = "Client";

        /// <summary>
        /// Specific profession type (e.g., "Plumber", "Developer") if UserType is Professional.
        /// </summary>
        public string? ProfessionType { get; set; }

        /// <summary>
        /// Timestamp when the account was created.
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Timestamp when the account was last updated.
        /// </summary>
        public DateTime? UpdatedAt { get; set; }
        
        /// <summary>
        /// Date the user joined the platform.
        /// </summary>
        public DateTime DateJoined { get; set; }

        // The following properties appear to be legacy or redundant with the Professional model.
        // They are kept for backward compatibility but should ideally be accessed via the Professional entity.
        
        public string? Specialization { get; set; }
        public int? Experience { get; set; }
        public string? Bio { get; set; }
        public string? ProfessionalId { get; set; }
    }

    /// <summary>
    /// Represents a role in the identity system (e.g., "Admin", "User").
    /// </summary>
    [CollectionName("Roles")]
    public class ApplicationRole : MongoIdentityRole<Guid>
    {
    }
}
