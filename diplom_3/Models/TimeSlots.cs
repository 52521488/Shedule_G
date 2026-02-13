using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// 9. TimeSlots.cs
namespace diplom_3.Models
{
    public class TimeSlot
    {
        public int Id { get; set; }
        public byte WeekDay { get; set; }          // 1..6
        public byte PairNumber { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public byte WeekParity { get; set; } = 0;
        public string Display => $"{PairNumber}. {StartTime:hh\\:mm} – {EndTime:hh\\:mm}";
        public virtual ICollection<ScheduleEntry> ScheduleEntries { get; set; } = new List<ScheduleEntry>();
    }
}