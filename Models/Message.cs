using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace ProMeet.Models
{
    public class Message
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }
        
        [BsonElement("messageId")]
        public int MessageID { get; set; }
        
        [BsonElement("chatId")]
        public int ChatID { get; set; }
        
        [BsonElement("senderId")]
        public int SenderID { get; set; }
        
        [BsonElement("senderType")]
        public string SenderType { get; set; } = string.Empty; // "Client" or "Professional"
        
        [BsonElement("content")]
        public string Content { get; set; } = string.Empty;
        
        [BsonElement("timestamp")]
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        
        [BsonElement("isRead")]
        public bool IsRead { get; set; } = false;
        
        [BsonElement("createdAt")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        [BsonElement("updatedAt")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        
        // Nested document for sender information
        [BsonElement("sender")]
        public ApplicationUser? Sender { get; set; }
    }
}
