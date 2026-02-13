using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// 4. Disciplines.cs
namespace diplom_3.Models
{
    public class Discipline
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string? ShortName { get; set; }
        public string? Code { get; set; }

        public virtual ICollection<TeacherDiscipline> TeacherDisciplines { get; set; } = new List<TeacherDiscipline>();
        public virtual ICollection<GroupDisciplineLoad> GroupDisciplineLoads { get; set; } = new List<GroupDisciplineLoad>();
        public virtual ICollection<ScheduleEntry> ScheduleEntries { get; set; } = new List<ScheduleEntry>();
    }
}