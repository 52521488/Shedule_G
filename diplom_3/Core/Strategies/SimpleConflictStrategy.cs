using System.Collections.Generic;
using System.Linq;
using diplom_3.Models;

namespace diplom_3.Core.Strategies
{
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