using System;
using System.ComponentModel.DataAnnotations;

namespace ProMeet.Models
{
    /// <summary>
    /// Représente un utilisateur de la plateforme.
    /// </summary>
    public class User
    {
        /// <summary>
        /// Identifiant de l'utilisateur.
        /// </summary>
        [Key]
        public int UserID { get; set; }

        /// <summary>
        /// Nom de l'utilisateur.
        /// </summary>
        [Required]
        [StringLength(50)]
        public string Name { get; set; }

        /// <summary>
        /// Adresse email de l'utilisateur.
        /// </summary>
        [Required]
        [EmailAddress]
        [StringLength(100)]
        public string Email { get; set; }

        /// <summary>
        /// Mot de passe de l'utilisateur.
        /// </summary>
        [Required]
        [StringLength(100)]
        public string Password { get; set; }

        /// <summary>
        /// Ville de résidence.
        /// </summary>
        [StringLength(50)]
        public string City { get; set; }

        /// <summary>
        /// Pays de résidence.
        /// </summary>
        [StringLength(50)]
        public string Country { get; set; }

        /// <summary>
        /// URL de la photo de profil.
        /// </summary>
        [StringLength(200)]
        public string PhotoURL { get; set; }

        /// <summary>
        /// Date d'inscription sur la plateforme.
        /// </summary>
        [Required]
        public DateTime DateJoined { get; set; }

        /// <summary>
        /// Liste des profils professionnels associés à l'utilisateur.
        /// </summary>
        public ICollection<Professional> Professionals { get; set; }

        /// <summary>
        /// Liste des rendez-vous de l'utilisateur.
        /// </summary>
        public ICollection<Appointment> Appointments { get; set; }

        /// <summary>
        /// Liste des conversations de l'utilisateur.
        /// </summary>
        public ICollection<Chat> Chats { get; set; }
    }
}
