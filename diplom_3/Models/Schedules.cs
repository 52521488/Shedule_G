using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// 10. Schedules.cs
namespace diplom_3.Models
{
    public class Schedule
    {
        public int Id { get; set; }
        public string Title { get; set; } = null!;
        public short AcademicYear { get; set; }     // 2025, 2026...
        public byte Semester { get; set; }
        public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
        public double? FitnessScore { get; set; }
        public bool IsApproved { get; set; }

        public virtual ICollection<ScheduleEntry> Entries { get; set; } = new List<ScheduleEntry>();
    }
}