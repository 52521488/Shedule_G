using System.Collections.Generic;
using System.Threading.Tasks;
using diplom_3.Models;

namespace diplom_3.Core.Interfaces
{
    public interface IScheduleService
    {
        Task<List<StudentGroup>> GetAllGroupsAsync();
        Task<List<Teacher>> GetAllTeachersAsync();
        Task<List<Discipline>> GetAllDisciplinesAsync();
        Task<List<Room>> GetAllRoomsAsync();
        Task<List<TimeSlot>> GetAllTimeSlotsAsync();

        Task SaveLessonAsync(LessonDto lesson);
        Task UpdateLessonAsync(LessonDto lesson);
    }
}