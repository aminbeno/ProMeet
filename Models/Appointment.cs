using System.ComponentModel.DataAnnotations;

namespace ProMeet.Models
{
    /// <summary>
    /// Représente un rendez-vous entre un client et un professionnel.
    /// </summary>
    public class Appointment
    {
        /// <summary>
        /// Identifiant du rendez-vous.
        /// </summary>
        [Key]
        public int AppointmentID { get; set; }

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
        /// Date du rendez-vous.
        /// </summary>
        [Required]
        public DateTime Date { get; set; }

        /// <summary>
        /// Heure de début du rendez-vous.
        /// </summary>
        [Required]
        public TimeSpan StartTime { get; set; }

        /// <summary>
        /// Heure de fin du rendez-vous.
        /// </summary>
        [Required]
        public TimeSpan EndTime { get; set; }

        /// <summary>
        /// Statut du rendez-vous.
        /// </summary>
        [Required]
        public AppointmentStatus Status { get; set; }

        /// <summary>
        /// Indique si le client a été notifié.
        /// </summary>
        public bool Notified { get; set; }

        /// <summary>
        /// Informations sur le client.
        /// </summary>
        public User Client { get; set; }

        /// <summary>
        /// Informations sur le professionnel.
        /// </summary>
        public Professional Professional { get; set; }

        /// <summary>
        /// Avis associé au rendez-vous.
        /// </summary>
        public Review Review { get; set; }
    }

    /// <summary>
    /// Enumération des statuts possibles pour un rendez-vous.
    /// </summary>
    public enum AppointmentStatus
    {
        Pending,
        Confirmed,
        Canceled
    }
}
