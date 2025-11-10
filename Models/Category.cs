using System.ComponentModel.DataAnnotations;

namespace ProMeet.Models
{
    /// <summary>
    /// Représente une catégorie professionnelle.
    /// </summary>
    public class Category
    {
        /// <summary>
        /// Identifiant de la catégorie.
        /// </summary>
        [Key]
        public int CategoryID { get; set; }

        /// <summary>
        /// Nom de la catégorie.
        /// </summary>
        [Required]
        [StringLength(50)]
        public string Name { get; set; }

        /// <summary>
        /// Liste des professionnels associés à cette catégorie.
        /// </summary>
        public ICollection<Professional> Professionals { get; set; }
    }
}
