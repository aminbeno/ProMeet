using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;

namespace ProMeet.Models
{
    /// <summary>
    /// Represents a professional category (e.g., "Medical", "IT", "Legal").
    /// </summary>
    public class Category
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
        [BsonElement("categoryId")]
        public int CategoryID { get; set; }
        
        /// <summary>
        /// Name of the category.
        /// </summary>
        [BsonElement("name")]
        public string Name { get; set; } = string.Empty;
        
        /// <summary>
        /// Description of the category.
        /// </summary>
        [BsonElement("description")]
        public string? Description { get; set; }
        
        /// <summary>
        /// URL to the category image/icon.
        /// </summary>
        [BsonElement("imageUrl")]
        public string? ImageUrl { get; set; }
        
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
    }
}
