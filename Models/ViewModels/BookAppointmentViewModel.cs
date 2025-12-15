using System;
using System.ComponentModel.DataAnnotations;
using ProMeet.Models;

namespace ProMeet.Models.ViewModels
{
    public class BookAppointmentViewModel
    {
        public string ProfessionalID { get; set; }
        
        public string? ServiceID { get; set; }
        public string? ServiceName { get; set; }

        public ApplicationUser? User { get; set; }

        public string? JobTitle { get; set; }

        public decimal Price { get; set; }

        public float Rating { get; set; }

        public string? ConsultationType { get; set; }

        [Required(ErrorMessage = "Please select a date.")]
        public DateTime? Date { get; set; }

        [Required(ErrorMessage = "Please select a time slot.")]
        public string TimeSlot { get; set; }

        public string? Reason { get; set; }

        public List<int> OffDays { get; set; } = new List<int>();
        
        // List of specific dates (as strings "yyyy-MM-dd") that are OFF
        public List<string> OffDates { get; set; } = new List<string>();
    }
}