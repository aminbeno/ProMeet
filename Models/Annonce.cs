using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.ComponentModel.DataAnnotations;

namespace ProMeet.Models
{
    public class Annonce
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        public string ProfessionalId { get; set; }

        [Required]
        public string Title { get; set; }

        [Required]
        public string Category { get; set; }

        [Required]
        public string Specialty { get; set; }

        [Required]
        public string Description { get; set; }

        [Required]
        [Range(0, 50)]
        public int YearsOfExperience { get; set; }

        public string Location { get; set; }

        [Required]
        public string Availability { get; set; }
    }
}