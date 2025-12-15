using System;
using System.Collections.Generic;

namespace ProMeet.ViewModels
{
    public class CalendarViewModel
    {
        public List<CalendarDay> Days { get; set; } = new List<CalendarDay>();
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }

    public class CalendarDay
    {
        public DateTime Date { get; set; }
        public bool IsAvailable { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public bool IsPast { get; set; }
        public bool IsToday { get; set; }
        public bool HasOverride { get; set; }
    }
}
