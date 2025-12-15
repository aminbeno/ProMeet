using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;

namespace ProMeet.Models
{
    public class Notification
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        [BsonElement("userId")]
        public string UserId { get; set; } // The user who receives the notification

        [BsonElement("title")]
        public string Title { get; set; } = string.Empty;

        [BsonElement("message")]
        public string Message { get; set; } = string.Empty;

        [BsonElement("type")]
        public NotificationType Type { get; set; }

        [BsonElement("relatedId")]
        public string? RelatedId { get; set; } // ID of the related entity (e.g., AppointmentId)

        [BsonElement("isRead")]
        public bool IsRead { get; set; } = false;

        [BsonElement("createdAt")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }

    public enum NotificationType
    {
        General,
        Appointment,
        Review,
        System
    }
}
