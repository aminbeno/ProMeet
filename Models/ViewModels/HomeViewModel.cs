using System.Collections.Generic;
using ProMeet.Models;

namespace ProMeet.Models.ViewModels
{
    public class HomeViewModel
    {
        public List<Professional> FeaturedProfessionals { get; set; } = new List<Professional>();
        public List<Review> RecentReviews { get; set; } = new List<Review>();
        public int TotalProfessionals { get; set; }
        public int TotalClients { get; set; } // Assuming "Users" who are not professionals
        public double AverageRating { get; set; }
    }
}
