using System.ComponentModel.DataAnnotations;

namespace ProMeet.Models
{
    /// <summary>
    /// Représente une conversation entre un client et un professionnel.
    /// </summary>
    public class Chat
    {
        /// <summary>
        /// Identifiant de la conversation.
        /// </summary>
        [Key]
        public int ChatID { get; set; }

        /// <summary>
        /// Identifiant du client.
        /// </summary>
        [Required]
        public int ClientID { get; set; }

        /// <summary>
        /// Identifiant du professionnel.
        /// </summary>
        [Required]
        public int ProfessionalID { get; set; }

        /// <summary>
        /// Date de début de la conversation.
        /// </summary>
        [Required]
        public DateTime DateStarted { get; set; }

        /// <summary>
        /// Informations sur le client.
        /// </summary>
        public User Client { get; set; }

        /// <summary>
        /// Informations sur le professionnel.
        /// </summary>
        public Professional Professional { get; set; }

        /// <summary>
        /// Liste des messages échangés dans la conversation.
        /// </summary>
        public ICollection<Message> Messages { get; set; }
    }
}
