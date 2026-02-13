using diplom_3.Core.Interfaces;
using diplom_3.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace diplom_3.Core.Services
{
    public class ScheduleService : IScheduleService
    {
        private readonly CollegeScheduleContext _context;

        public ScheduleService(CollegeScheduleContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        // ──────────────────────────────────────────────────────────────
        // Чтение справочников
        // ──────────────────────────────────────────────────────────────

        public async Task<List<StudentGroup>> GetAllGroupsAsync()
        {
            return await _context.StudentGroups
                .OrderBy(g => g.Name)
                .ToListAsync();
        }

        public async Task<List<Teacher>> GetAllTeachersAsync()
        {
            return await _context.Teachers
                .Where(t => t.IsActive)
                .OrderBy(t => t.ShortName)
                .ToListAsync();
        }

        public async Task<List<Discipline>> GetAllDisciplinesAsync()
        {
            return await _context.Disciplines
                .OrderBy(d => d.ShortName ?? d.Name)
                .ToListAsync();
        }

        public async Task<List<Room>> GetAllRoomsAsync()
        {
            return await _context.Rooms
                .OrderBy(r => r.Name)
                .ToListAsync();
        }

        public async Task<List<TimeSlot>> GetAllTimeSlotsAsync()
        {
            return await _context.TimeSlots
                .OrderBy(ts => ts.WeekDay)
                .ThenBy(ts => ts.PairNumber)
                .ToListAsync();
        }

        // ──────────────────────────────────────────────────────────────
        // Работа с занятиями (CRUD базовый)
        // ──────────────────────────────────────────────────────────────

        /// <summary>
        /// Добавляет новое занятие в базу.
        /// Предполагается, что ScheduleId уже известен (текущее расписание).
        /// Если нет — нужно либо передавать его, либо создавать новый Schedule.
        /// </summary>
        public async Task<long> SaveLessonAsync(LessonDto dto, int scheduleId)
        {
            if (dto == null) throw new ArgumentNullException(nameof(dto));

            var entry = new ScheduleEntry
            {
                ScheduleId = scheduleId,  // ← критично! откуда брать?
                TimeSlotId = dto.TimeSlotId ?? throw new ArgumentException("Нет TimeSlotId"),
                GroupId = dto.GroupId ?? throw new ArgumentException("Нет GroupId"),
                SubGroupId = dto.SubGroupId,
                TeacherId = dto.TeacherId ?? throw new ArgumentException("Нет TeacherId"),
                DisciplineId = dto.DisciplineId ?? throw new ArgumentException("Нет DisciplineId"),
                RoomId = dto.RoomId ?? throw new ArgumentException("Нет RoomId"),
            };

            _context.ScheduleEntries.Add(entry);
            await _context.SaveChangesAsync();

            return entry.Id;  // возвращаем сгенерированный Id
        }

        /// <summary>
        /// Обновляет существующее занятие по Id из DTO.
        /// </summary>
        public async Task UpdateLessonAsync(LessonDto dto)
        {
            if (dto == null) throw new ArgumentNullException(nameof(dto));
            if (dto.Id <= 0) throw new ArgumentException("Некорректный Id для обновления");

            var entry = await _context.ScheduleEntries
                .FirstOrDefaultAsync(e => e.Id == dto.Id);

            if (entry == null)
                throw new KeyNotFoundException($"Занятие с Id {dto.Id} не найдено");

            // обновляем только те поля, которые переданы
            if (dto.TimeSlotId.HasValue) entry.TimeSlotId = dto.TimeSlotId.Value;
            if (dto.GroupId.HasValue) entry.GroupId = dto.GroupId.Value;
            if (dto.SubGroupId.HasValue) entry.SubGroupId = dto.SubGroupId.Value;
            if (dto.TeacherId.HasValue) entry.TeacherId = dto.TeacherId.Value;
            if (dto.DisciplineId.HasValue) entry.DisciplineId = dto.DisciplineId.Value;
            if (dto.RoomId.HasValue) entry.RoomId = dto.RoomId.Value;

            await _context.SaveChangesAsync();
        }

        // ──────────────────────────────────────────────────────────────
        // Получение занятий (пример — по группе и неделе)
        // ──────────────────────────────────────────────────────────────

        public async Task<List<LessonDto>> GetLessonsForGroupAsync(
            int groupId,
            int weekParity = 0,          // 0 = обе, 1 = верх, 2 = низ
            int? scheduleId = null)      // если работаем с конкретным расписанием
        {
            var query = _context.ScheduleEntries
                .Include(e => e.TimeSlot)
                .Include(e => e.Group)
                .Include(e => e.Teacher)
                .Include(e => e.Discipline)
                .Include(e => e.Room)
                .Where(e => e.GroupId == groupId);

            if (weekParity > 0)
            {
                query = query.Where(e => e.TimeSlot!.WeekParity == 0 || e.TimeSlot.WeekParity == weekParity);
            }

            if (scheduleId.HasValue)
            {
                query = query.Where(e => e.ScheduleId == scheduleId.Value);
            }

            var entries = await query.ToListAsync();

            return entries.Select(e => new LessonDto
            {
                Id = (int)e.Id,
                Day = GetDayName(e.TimeSlot!.WeekDay),
                TimeSlot = $"{e.TimeSlot.StartTime:hh\\:mm} – {e.TimeSlot.EndTime:hh\\:mm}",
                Room = e.Room?.Name ?? "—",
                Subject = e.Discipline?.ShortName ?? e.Discipline?.Name ?? "?",
                Group = e.Group?.Name ?? "?",
                Teacher = e.Teacher?.ShortName ?? e.Teacher?.FullName ?? "?",
                WeekDayNumber = e.TimeSlot.WeekDay,
                PairNumber = e.TimeSlot.PairNumber,
                StartTime = e.TimeSlot.StartTime,
                EndTime = e.TimeSlot.EndTime,
                RoomId = e.RoomId,
                TeacherId = e.TeacherId,
                GroupId = e.GroupId,
                DisciplineId = e.DisciplineId,
                IsCanceled = false   // пока нет флага отмены в базе
            })
            .OrderBy(l => l.WeekDayNumber)
            .ThenBy(l => l.PairNumber)
            .ToList();
        }

        private static string GetDayName(int weekDay) => weekDay switch
        {
            1 => "Понедельник",
            2 => "Вторник",
            3 => "Среда",
            4 => "Четверг",
            5 => "Пятница",
            6 => "Суббота",
            7 => "Воскресенье",
            _ => "???"
        };

        Task<List<StudentGroup>> IScheduleService.GetAllGroupsAsync()
        {
            throw new NotImplementedException();
        }

        Task<List<Teacher>> IScheduleService.GetAllTeachersAsync()
        {
            throw new NotImplementedException();
        }

        Task<List<Discipline>> IScheduleService.GetAllDisciplinesAsync()
        {
            throw new NotImplementedException();
        }

        Task<List<Room>> IScheduleService.GetAllRoomsAsync()
        {
            throw new NotImplementedException();
        }

        Task<List<TimeSlot>> IScheduleService.GetAllTimeSlotsAsync()
        {
            throw new NotImplementedException();
        }

        Task IScheduleService.SaveLessonAsync(LessonDto lesson)
        {
            throw new NotImplementedException();
        }

        Task IScheduleService.UpdateLessonAsync(LessonDto lesson)
        {
            throw new NotImplementedException();
        }
    }
}