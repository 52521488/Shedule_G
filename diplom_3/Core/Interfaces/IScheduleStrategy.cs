using System.Collections.Generic;
using diplom_3.Models;

namespace diplom_3.Core.Interfaces
{
    public interface IScheduleStrategy
    {
        bool HasConflict(LessonDto candidate, IEnumerable<LessonDto> existing);
    }
}
