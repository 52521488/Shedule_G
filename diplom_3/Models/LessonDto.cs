namespace diplom_3.Models
{
    public class LessonDto
    {
        public long Id { get; set; }                    // изменил на long, т.к. Id в базе BIGINT

        public string Day { get; set; } = "";
        public string TimeSlot { get; set; } = "";
        public string Room { get; set; } = "";
        public string Subject { get; set; } = "";
        public string Group { get; set; } = "";
        public string Teacher { get; set; } = "";

        // Обязательные для базы — FK
        public int? TimeSlotId { get; set; }
        public int? GroupId { get; set; }
        public int? SubGroupId { get; set; }          // nullable, если подгруппы не всегда
        public int? TeacherId { get; set; }
        public int? DisciplineId { get; set; }
        public int? RoomId { get; set; }

        // Для отображения и сортировки
        public int WeekDayNumber { get; set; }
        public int PairNumber { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public bool IsCanceled { get; set; }
        public string? CancelReason { get; set; }

        public string DisplayText =>
            $"{TimeSlot} {Subject}\n{Teacher} • {Room}";
    }
}