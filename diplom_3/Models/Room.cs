using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// 2. Rooms.cs
namespace diplom_3.Models
{
    public class Room
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public int BuildingId { get; set; }
        public short Capacity { get; set; } = 30;

        public virtual Building Building { get; set; } = null!;
        public virtual ICollection<ScheduleEntry> ScheduleEntries { get; set; } = new List<ScheduleEntry>();
    }
}
