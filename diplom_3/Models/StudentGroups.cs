using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// 6. StudentGroups.cs
namespace diplom_3.Models
{
    public class StudentGroup
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public byte Course { get; set; }
        public byte Semester { get; set; }
        public short? StudentsCount { get; set; }

        public virtual ICollection<SubGroup> SubGroups { get; set; } = new List<SubGroup>();
        public virtual ICollection<GroupDisciplineLoad> GroupDisciplineLoads { get; set; } = new List<GroupDisciplineLoad>();
        public virtual ICollection<ScheduleEntry> ScheduleEntries { get; set; } = new List<ScheduleEntry>();
    }
}