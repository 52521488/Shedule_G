using System;

namespace diplom_2.Models   // ← или просто diplom_2, если хочешь без папки
{
    public class LessonModel
    {
        public int Id { get; set; }                 // если потом перейдёшь на БД — пригодится
        public string Day { get; set; }             // "Понедельник"
        public string TimeSlot { get; set; }        // "8:30-10:00"
        public string Room { get; set; }            // "301"
        public string Teacher { get; set; }         // "Иванов И.И."
        public string Group { get; set; }           // "ИС-21"
        public string Subject { get; set; }         // "Математика"
        public bool IsCanceled { get; set; } = false; // для отмены пары
    }
}