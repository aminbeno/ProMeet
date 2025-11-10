using System.ComponentModel.DataAnnotations;

namespace ProMeet.Models
{
    /// <summary>
    /// Représente un avis laissé par un client après un rendez-vous.
    /// </summary>
    public class Review
    {
        /// <summary>
        /// Identifiant de l'avis.
        /// </summary>
        [Key]
        public int ReviewID { get; set; }

        /// <summary>
        /// Identifiant du rendez-vous associé.
        /// </summary>
        [Required]
        public int AppointmentID { get; set; }

        /// <summary>
        /// Note attribuée (1 à 5).
        /// </summary>
        [Required]
        [Range(1, 5)]
        public int Rating { get; set; }

        /// <summary>
        /// Commentaire du client.
        /// </summary>
        [StringLength(500)]
        public string Comment { get; set; }

        /// <summary>
        /// Date de soumission de l'avis.
        /// </summary>
        [Required]
        public DateTime DateProvided { get; set; }

        /// <summary>
        /// Informations sur le rendez-vous associé.
        /// </summary>
        public Appointment Appointment { get; set; }

        /// <summary>
        /// Informations sur le professionnel concerné.
        /// </summary>
        public Professional Professional { get; set; }
    }
}
