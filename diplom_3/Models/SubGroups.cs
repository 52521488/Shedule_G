using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// 7. SubGroups.cs
namespace diplom_3.Models
{
    public class SubGroup
    {
        public int Id { get; set; }
        public int GroupId { get; set; }
        public string Name { get; set; } = null!;
        public short StudentsCount { get; set; }

        public virtual StudentGroup Group { get; set; } = null!;
        public virtual ICollection<GroupDisciplineLoad> GroupDisciplineLoads { get; set; } = new List<GroupDisciplineLoad>();
        public virtual ICollection<ScheduleEntry> ScheduleEntries { get; set; } = new List<ScheduleEntry>();
    }
}