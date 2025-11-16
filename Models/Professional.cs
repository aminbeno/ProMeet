using System.ComponentModel.DataAnnotations;
using System;

namespace ProMeet.Models
{
    /// <summary>
    /// Représente un professionnel inscrit sur la plateforme.
    /// </summary>
    public class Professional
    {
        /// <summary>
        /// Identifiant du professionnel.
        /// </summary>
        [Key]
        public int ProfessionalID { get; set; }

        /// <summary>
        /// Identifiant de l'utilisateur associé.
        /// </summary>
        [Required]
        public int UserID { get; set; }

        /// <summary>
        /// Intitulé du poste ou métier.
        /// </summary>
        [Required]
        [StringLength(50)]
        public string JobTitle { get; set; }

        /// <summary>
        /// Spécialité du professionnel.
        /// </summary>
        [Required]
        [StringLength(100)]
        public string Specialty { get; set; }

        /// <summary>
        /// Biographie du professionnel.
        /// </summary>
        [StringLength(500)]
        public string Bio { get; set; }

        /// <summary>
        /// Expérience professionnelle.
        /// </summary>
        [StringLength(200)]
        public string Experience { get; set; }

        /// <summary>
        /// Diplômes obtenus.
        /// </summary>
        [StringLength(200)]
        public string Degrees { get; set; }

        /// <summary>
        /// Type de consultation ("online" ou "in-person").
        /// </summary>
        [Required]
        [StringLength(20)]
        public string ConsultationType { get; set; }

        /// <summary>
        /// Prix de la consultation.
        /// </summary>
        [Range(0, 10000)]
        public decimal Price { get; set; }

        /// <summary>
        /// Indique si le profil est validé par l'administration.
        /// </summary>
        public bool IsValidated { get; set; }

        /// <summary>
        /// Note moyenne du professionnel.
        /// </summary>
        [Range(0, 5)]
        public float Rating { get; set; }

        /// <summary>
        /// Indique si le profil est actif.
        /// </summary>
        public bool ProfileActive { get; set; }

        /// <summary>
        /// Informations sur l'utilisateur associé.
        /// </summary>
        public User User { get; set; }

        /// <summary>
        /// Identifiant de la catégorie professionnelle.
        /// </summary>
        [Required]
        public int CategoryID { get; set; }

        /// <summary>
        /// Catégorie professionnelle associée.
        /// </summary>
        public Category Category { get; set; }

        /// <summary>
        /// Disponibilités du professionnel.
        /// </summary>
        public ICollection<Availability> Availabilities { get; set; }

        /// <summary>
        /// Rendez-vous du professionnel.
        /// </summary>
        public ICollection<Appointment> Appointments { get; set; }

        /// <summary>
        /// Avis reçus par le professionnel.
        /// </summary>
        public ICollection<Review> Reviews { get; set; }

        /// <summary>
        /// Conversations du professionnel.
        /// </summary>
        public ICollection<Chat> Chats { get; set; }
    }
}
