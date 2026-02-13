using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// 5. TeacherDisciplines.cs  (junction table — многие-ко-многим)
namespace diplom_3.Models
{
    public class TeacherDiscipline
    {
        public int TeacherId { get; set; }
        public int DisciplineId { get; set; }

        public virtual Teacher Teacher { get; set; } = null!;
        public virtual Discipline Discipline { get; set; } = null!;
    }
}