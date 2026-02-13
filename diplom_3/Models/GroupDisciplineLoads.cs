using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// 8. GroupDisciplineLoads.cs
namespace diplom_3.Models
{
    public class GroupDisciplineLoad
    {
        public int Id { get; set; }
        public int GroupId { get; set; }
        public int? SubGroupId { get; set; }
        public int DisciplineId { get; set; }
        public short TotalHours { get; set; }
        public byte WeekParity { get; set; } = 0;   // 0=обе, 1=верх, 2=низ

        public virtual StudentGroup Group { get; set; } = null!;
        public virtual SubGroup? SubGroup { get; set; }
        public virtual Discipline Discipline { get; set; } = null!;
    }
}