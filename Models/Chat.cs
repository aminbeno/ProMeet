using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace ProMeet.Models
{
    public class Chat
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }
        
        [BsonElement("chatId")]
        public int ChatID { get; set; }
        
        [BsonElement("clientId")]
        public int ClientID { get; set; }
        
        [BsonElement("professionalId")]
        public int ProfessionalID { get; set; }
        
        [BsonElement("createdAt")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        [BsonElement("updatedAt")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        
        [BsonElement("isActive")]
        public bool IsActive { get; set; } = true;
        
        // Backward compatibility properties
        [BsonIgnore]
        public DateTime DateStarted { get => CreatedAt; set => CreatedAt = value; }
        
        [BsonIgnore]
        public DateTime? LastMessageAt { get; set; }
        
        // Nested documents for related data
        [BsonElement("client")]
        public ApplicationUser? Client { get; set; }
        
        [BsonElement("professional")]
        public Professional? Professional { get; set; }
        
        // Embedded messages collection
        [BsonElement("messages")]
        public List<Message> Messages { get; set; } = new List<Message>();
    }
}
