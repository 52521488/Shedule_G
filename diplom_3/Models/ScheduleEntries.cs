using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// 11. ScheduleEntries.cs
namespace diplom_3.Models
{
    public class ScheduleEntry
    {
        public long Id { get; set; }                // BIGINT

        public int ScheduleId { get; set; }
        public int TimeSlotId { get; set; }
        public int GroupId { get; set; }
        public int? SubGroupId { get; set; }
        public int TeacherId { get; set; }
        public int DisciplineId { get; set; }
        public int RoomId { get; set; }

        public virtual Schedule Schedule { get; set; } = null!;
        public virtual TimeSlot TimeSlot { get; set; } = null!;
        public virtual StudentGroup Group { get; set; } = null!;
        public virtual SubGroup? SubGroup { get; set; }
        public virtual Teacher Teacher { get; set; } = null!;
        public virtual Discipline Discipline { get; set; } = null!;
        public virtual Room Room { get; set; } = null!;
    }
}