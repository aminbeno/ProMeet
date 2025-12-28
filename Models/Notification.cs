using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;

namespace ProMeet.Models
{
    /// <summary>
    /// Represents a system or user notification.
    /// </summary>
    public class Notification
    {
        /// <summary>
        /// Unique MongoDB Identifier.
        /// </summary>
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        /// <summary>
        /// ID of the user who receives the notification.
        /// </summary>
        [BsonElement("userId")]
        public string UserId { get; set; } 

        /// <summary>
        /// Short title of the notification.
        /// </summary>
        [BsonElement("title")]
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// Full body message.
        /// </summary>
        [BsonElement("message")]
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// Category/Type of the notification.
        /// </summary>
        [BsonElement("type")]
        public NotificationType Type { get; set; }

        /// <summary>
        /// ID of the related entity (e.g., AppointmentId) for deep linking.
        /// </summary>
        [BsonElement("relatedId")]
        public string? RelatedId { get; set; } 

        /// <summary>
        /// Indicates if the notification has been read by the user.
        /// </summary>
        [BsonElement("isRead")]
        public bool IsRead { get; set; } = false;

        /// <summary>
        /// Timestamp of creation.
        /// </summary>
        [BsonElement("createdAt")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }

    /// <summary>
    /// Enumeration of notification types.
    /// </summary>
    public enum NotificationType
    {
        General,
        Appointment,
        Review,
        System
    }
}
