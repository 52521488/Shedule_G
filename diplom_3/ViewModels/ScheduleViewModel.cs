using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using diplom_3.Models;

namespace diplom_3
{
    public class ScheduleViewModel : INotifyPropertyChanged
    {
        public ObservableCollection<LessonDto> Lessons { get; } = new ObservableCollection<LessonDto>();

        public List<string> Days { get; } = new List<string>
        {
            "Понедельник", "Вторник", "Среда", "Четверг", "Пятница", "Суббота", "Воскресенье"
        };

        public ObservableCollection<string> TimeSlots { get; } = new ObservableCollection<string>
        {
            "8:00-9:30", "9:40-11:10", "11:20-12:50", "13:20-14:50",
            "15:00-16:30", "16:40-18:10", "18:10-19:40"
        };

        public double ZoomLevel { get; set; } = 1.0;

        public ICommand AddLessonCommand { get; }

        public ScheduleViewModel()
        {
            // Тестовые данные УБРАТЬ ПОТОМ!!!
            Lessons.Add(new LessonDto { Day = "Понедельник", TimeSlot = "8:30-10:00", Subject = "Математика", Group = "ИС-21", Teacher = "Иванов И.И.", Room = "301" });
            Lessons.Add(new LessonDto { Day = "Вторник", TimeSlot = "10:10-11:40", Subject = "Программирование", Group = "ИС-21", Teacher = "Петров П.П.", Room = "405" });

            AddLessonCommand = new RelayCommand(() => AddLesson());
        }

        private void AddLesson()
        {
            // Здесь будет вызов окна добавления — пока просто заглушка
            // Реальная реализация будет в MainWindow
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}