using System.ComponentModel.DataAnnotations;

namespace ProMeet.Models
{
    /// <summary>
    /// Représente la disponibilité d'un professionnel pour un jour donné.
    /// </summary>
    public class Availability
    {
        /// <summary>
        /// Identifiant de la disponibilité.
        /// </summary>
        [Key]
        public int AvailabilityID { get; set; }

        /// <summary>
        /// Identifiant du professionnel.
        /// </summary>
        [Required]
        public int ProfessionalID { get; set; }

        /// <summary>
        /// Jour de la semaine (ex: "Lundi").
        /// </summary>
        [Required]
        [StringLength(10)]
        public string DayOfWeek { get; set; }

        /// <summary>
        /// Heure de début de disponibilité.
        /// </summary>
        [Required]
        public TimeSpan StartHour { get; set; }

        /// <summary>
        /// Heure de fin de disponibilité.
        /// </summary>
        [Required]
        public TimeSpan EndHour { get; set; }

        /// <summary>
        /// Indique si c'est un jour de repos.
        /// </summary>
        public bool IsRestDay { get; set; }

        /// <summary>
        /// Informations sur le professionnel.
        /// </summary>
        public Professional Professional { get; set; }
    }
}
