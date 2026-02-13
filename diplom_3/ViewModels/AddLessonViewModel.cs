using diplom_2.Core;
using diplom_2.Core.Strategies;
using diplom_2.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;

namespace diplom_2
{
    public class AddLessonViewModel : INotifyPropertyChanged
    {
        public string WindowTitle { get; } = "Добавить занятие";

        public List<string> Days { get; } = new List<string>
        {
            "Понедельник", "Вторник", "Среда", "Четверг", "Пятница", "Суббота", "Воскресенье"
        };

        public ObservableCollection<string> TimeSlots { get; } = new ObservableCollection<string>
        {
            "8:30-10:00", "10:10-11:40", "11:50-13:20", "13:30-15:00",
            "15:10-16:40", "16:50-18:20", "18:30-20:00"
        };

        // =====================================================================
        // Свойства с биндингом
        // =====================================================================
        private string _selectedDay;
        public string SelectedDay
        {
            get => _selectedDay;
            set { _selectedDay = value; OnPropertyChanged(); }
        }

        private string _selectedTimeSlot;
        public string SelectedTimeSlot
        {
            get => _selectedTimeSlot;
            set { _selectedTimeSlot = value; OnPropertyChanged(); }
        }

        public string Room { get; set; }
        public string Subject { get; set; }
        public string Group { get; set; }
        public string Teacher { get; set; }

        // =====================================================================
        // Команды
        // =====================================================================
        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }

        // =====================================================================
        // Зависимости
        // =====================================================================
        private readonly ObservableCollection<LessonModel> _lessons;
        private readonly Action _notifyUpdate;
        private readonly IScheduleStrategy _conflictChecker;
        private readonly LessonModel _editingLesson; // null при добавлении

        public AddLessonViewModel(
            ObservableCollection<LessonModel> lessons,
            Action notifyUpdate = null,
            IScheduleStrategy conflictChecker = null,
            LessonModel editingLesson = null)
        {
            _lessons = lessons ?? throw new ArgumentNullException(nameof(lessons));
            _notifyUpdate = notifyUpdate;
            _conflictChecker = conflictChecker ?? new Core.SimpleConflictStrategy();
            _editingLesson = editingLesson;

            // Если редактируем — заполняем поля
            if (editingLesson != null)
            {
                WindowTitle = "Редактировать занятие";
                SelectedDay = editingLesson.Day;
                SelectedTimeSlot = editingLesson.TimeSlot;
                Room = editingLesson.Room;
                Subject = editingLesson.Subject;
                Group = editingLesson.Group;
                Teacher = editingLesson.Teacher;
            }

            SaveCommand = new RelayCommand(Save, CanSave);
            CancelCommand = new RelayCommand(Cancel);
        }

        private bool CanSave()
        {
            return !string.IsNullOrWhiteSpace(SelectedDay) &&
                   !string.IsNullOrWhiteSpace(SelectedTimeSlot) &&
                   !string.IsNullOrWhiteSpace(Room);
        }

        private void Save()
        {
            if (!CanSave())
            {
                MessageBox.Show("Заполни день, время и аудиторию хотя бы", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var lesson = new LessonModel
            {
                Day = SelectedDay.Trim(),
                TimeSlot = SelectedTimeSlot.Trim(),
                Room = Room?.Trim(),
                Subject = Subject?.Trim() ?? "Не указано",
                Group = Group?.Trim() ?? "Не указана",
                Teacher = Teacher?.Trim() ?? "Не указан",
                IsCanceled = _editingLesson?.IsCanceled ?? false
            };

            // Проверяем конфликт (исключаем себя при редактировании)
            var others = _lessons.Where(l => l != _editingLesson);
            if (_conflictChecker.HasConflict(lesson, others))
            {
                MessageBox.Show("Конфликт расписания!\nАудитория / препод / группа уже занята.", "Конфликт", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (_editingLesson != null)
            {
                // Редактирование — заменяем объект
                var index = _lessons.IndexOf(_editingLesson);
                _lessons[index] = lesson;
            }
            else
            {
                // Добавление
                _lessons.Add(lesson);
            }

            // Уведомляем главное окно
            _notifyUpdate?.Invoke();

            // Закрываем окно
            CloseWindow(true);
        }

        private void Cancel()
        {
            CloseWindow(false);
        }

        private void CloseWindow(bool success)
        {
            var window = Application.Current.Windows.OfType<Window>()
                .FirstOrDefault(w => w.DataContext == this);

            if (window != null)
            {
                window.DialogResult = success;
                window.Close();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}