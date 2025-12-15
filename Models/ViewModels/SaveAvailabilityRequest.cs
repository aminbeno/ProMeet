using System;

namespace ProMeet.Models.ViewModels
{
    public class SaveAvailabilityRequest
    {
        public string Date { get; set; } // "yyyy-MM-dd"
        public bool IsAvailable { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
    }
}
