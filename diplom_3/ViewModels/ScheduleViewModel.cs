using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using diplom_2.Models;

namespace diplom_2
{
    public class ScheduleViewModel : INotifyPropertyChanged
    {
        public ObservableCollection<LessonModel> Lessons { get; } = new ObservableCollection<LessonModel>();

        public List<string> Days { get; } = new List<string>
        {
            "Понедельник", "Вторник", "Среда", "Четверг", "Пятница", "Суббота", "Воскресенье"
        };

        public ObservableCollection<string> TimeSlots { get; } = new ObservableCollection<string>
        {
            "8:30-10:00", "10:10-11:40", "11:50-13:20", "13:30-15:00",
            "15:10-16:40", "16:50-18:20", "18:30-20:00"
        };

        public double ZoomLevel { get; set; } = 1.0;

        public ICommand AddLessonCommand { get; }

        public ScheduleViewModel()
        {
            // Тестовые данные, потом уберёшь
            Lessons.Add(new LessonModel { Day = "Понедельник", TimeSlot = "8:30-10:00", Subject = "Математика", Group = "ИС-21", Teacher = "Иванов И.И.", Room = "301" });
            Lessons.Add(new LessonModel { Day = "Вторник", TimeSlot = "10:10-11:40", Subject = "Программирование", Group = "ИС-21", Teacher = "Петров П.П.", Room = "405" });

            AddLessonCommand = new RelayCommand(AddLesson);
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