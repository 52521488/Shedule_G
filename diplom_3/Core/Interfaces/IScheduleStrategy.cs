using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using diplom_3.Models;

namespace diplom_3.Core
{
    public interface IScheduleStrategy
    {
        bool HasConflict(LessonDto candidate, IEnumerable<LessonDto> existing);
    }

    public class SimpleConflictStrategy : IScheduleStrategy
    {
        public bool HasConflict(LessonDto candidate, IEnumerable<LessonDto> existing)
        {
            return existing.Any(l =>
                l.Day == candidate.Day &&
                l.TimeSlot == candidate.TimeSlot &&
                (l.Room == candidate.Room ||
                 l.Teacher == candidate.Teacher ||
                 l.Group == candidate.Group));
        }
    }
}
