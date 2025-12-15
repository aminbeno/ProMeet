using ProMeet.Models;
using System.Collections.Generic;

namespace ProMeet.ViewModels
{
    public class ProfessionalDashboardViewModel
    {
        public Professional Professional { get; set; }
        public int UpcomingAppointmentsCount { get; set; }
        public int TotalClients { get; set; }
        public double AverageRating { get; set; }
        public int ReviewsCount { get; set; }
        public IEnumerable<Appointment> RecentAppointments { get; set; }
    }
}
