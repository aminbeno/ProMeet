using System.ComponentModel.DataAnnotations;

namespace ProMeet.Models
{
    /// <summary>
    /// Représente un message échangé dans une conversation.
    /// </summary>
    public class Message
    {
        /// <summary>
        /// Identifiant du message.
        /// </summary>
        [Key]
        public int MessageID { get; set; }

        /// <summary>
        /// Identifiant de la conversation.
        /// </summary>
        [Required]
        public int ChatID { get; set; }

        /// <summary>
        /// Identifiant de l'expéditeur du message.
        /// </summary>
        [Required]
        public int SenderID { get; set; }

        /// <summary>
        /// Contenu du message.
        /// </summary>
        [Required]
        [StringLength(500)]
        public string Content { get; set; }

        /// <summary>
        /// Date et heure d'envoi du message.
        /// </summary>
        [Required]
        public DateTime Timestamp { get; set; }

        /// <summary>
        /// Informations sur la conversation.
        /// </summary>
        public Chat Chat { get; set; }

        /// <summary>
        /// Informations sur l'expéditeur.
        /// </summary>
        public User Sender { get; set; }
    }
}
