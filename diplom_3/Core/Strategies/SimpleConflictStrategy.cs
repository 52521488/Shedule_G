using System.Collections.Generic;
using System.Linq;
using diplom_2.Models;

namespace diplom_2.Core.Strategies
{
    public class SimpleConflictStrategy : IScheduleStrategy
    {
        public bool HasConflict(LessonModel candidate, IEnumerable<LessonModel> existing)
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