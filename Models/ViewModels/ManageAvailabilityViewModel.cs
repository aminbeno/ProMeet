using System;
using System.Collections.Generic;
using ProMeet.Models;

namespace ProMeet.ViewModels
{
    public class ManageAvailabilityViewModel
    {
        public List<DailySchedule> Schedule { get; set; } = new List<DailySchedule>();
    }

    public class DailySchedule
    {
        public int DayOfWeek { get; set; } // 0 = Sunday, 1 = Monday, etc.
        public string DayName { get; set; }
        public bool IsAvailable { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
    }
}
