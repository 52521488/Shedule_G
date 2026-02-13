using diplom_3.Core;
using diplom_3.Models;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using diplom_3.Core.Interfaces;

namespace diplom_3.ViewModels
{
    public class AddEditLessonViewModel : INotifyPropertyChanged
    {
        private readonly IScheduleService _scheduleService;          // основной сервис для БД
        private readonly IScheduleStrategy _conflictChecker;         // твой чекер конфликтов
        private readonly ObservableCollection<LessonDto> _lessons;   // ссылка на коллекцию главного окна
        private readonly Action _notifyParentUpdate;

        private LessonDto _editingLesson;  // null если добавляем

        public string WindowTitle { get; private set; } = "Добавить занятие";

        // Коллекции для ComboBox — загружаем из БД
        public ObservableCollection<string> Days { get; } = new()
        {
            "Понедельник", "Вторник", "Среда", "Четверг", "Пятница", "Суббота"
        };  // воскресенье редко, но можно добавить

        public ObservableCollection<TimeSlot> TimeSlots { get; } = new();
        public ObservableCollection<Room> Rooms { get; } = new();
        public ObservableCollection<Discipline> Disciplines { get; } = new();
        public ObservableCollection<StudentGroup> Groups { get; } = new();
        public ObservableCollection<Teacher> Teachers { get; } = new();

        // Выбранные значения (биндинг)
        private string _selectedDay;
        public string SelectedDay
        {
            get => _selectedDay;
            set { _selectedDay = value; OnPropertyChanged(); ValidateCanSave(); }
        }

        private TimeSlot _selectedTimeSlot;
        public TimeSlot SelectedTimeSlot
        {
            get => _selectedTimeSlot;
            set { _selectedTimeSlot = value; OnPropertyChanged(); ValidateCanSave(); }
        }

        private Room _selectedRoom;
        public Room SelectedRoom
        {
            get => _selectedRoom;
            set { _selectedRoom = value; OnPropertyChanged(); ValidateCanSave(); }
        }

        private Discipline _selectedDiscipline;
        public Discipline SelectedDiscipline
        {
            get => _selectedDiscipline;
            set { _selectedDiscipline = value; OnPropertyChanged(); /* можно фильтровать преподов */ }
        }

        private StudentGroup _selectedGroup;
        public StudentGroup SelectedGroup
        {
            get => _selectedGroup;
            set { _selectedGroup = value; OnPropertyChanged(); ValidateCanSave(); }
        }

        private Teacher _selectedTeacher;
        public Teacher SelectedTeacher
        {
            get => _selectedTeacher;
            set { _selectedTeacher = value; OnPropertyChanged(); ValidateCanSave(); }
        }

        // Статус валидации для кнопки
        private bool _canSave;
        public bool CanSave
        {
            get => _canSave;
            private set { _canSave = value; OnPropertyChanged(); }
        }

        // Команды
        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }

        public AddEditLessonViewModel(
            IScheduleService scheduleService,
            ObservableCollection<LessonDto> lessonsCollection,
            Action notifyParentUpdate = null,
            IScheduleStrategy conflictChecker = null,
            LessonDto editingLesson = null)
        {
            _scheduleService = scheduleService ?? throw new ArgumentNullException(nameof(scheduleService));
            _lessons = lessonsCollection ?? throw new ArgumentNullException(nameof(lessonsCollection));
            _notifyParentUpdate = notifyParentUpdate;
            _conflictChecker = conflictChecker ?? new SimpleConflictStrategy();
            _editingLesson = editingLesson;

            SaveCommand = new RelayCommand(async () => await SaveAsync(), () => CanSave);
            CancelCommand = new RelayCommand(() => Cancel());

            if (editingLesson != null)
            {
                WindowTitle = "Редактировать занятие";
                LoadFromEditingLesson(editingLesson);
            }

            _ = LoadDataAsync();  // асинхронно грузим всё из БД
        }

        private async Task LoadDataAsync()
        {
            try
            {
                // Загружаем реальные данные
                var timeSlots = await _scheduleService.GetAllTimeSlotsAsync(); // добавь метод в сервис
                var rooms = await _scheduleService.GetAllRoomsAsync();
                var disciplines = await _scheduleService.GetAllDisciplinesAsync();
                var groups = await _scheduleService.GetAllGroupsAsync();
                var teachers = await _scheduleService.GetAllTeachersAsync();

                TimeSlots.Clear(); foreach (var ts in timeSlots) TimeSlots.Add(ts);
                Rooms.Clear(); foreach (var r in rooms) Rooms.Add(r);
                Disciplines.Clear(); foreach (var d in disciplines) Disciplines.Add(d);
                Groups.Clear(); foreach (var g in groups) Groups.Add(g);
                Teachers.Clear(); foreach (var t in teachers) Teachers.Add(t);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Не удалось загрузить справочники:\n{ex.Message}", "Ошибка БД", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadFromEditingLesson(LessonDto lesson)
        {
            SelectedDay = lesson.Day;
            SelectedTimeSlot = TimeSlots.FirstOrDefault(ts => ts.Display == lesson.TimeSlot); // подбери по строке
            SelectedRoom = Rooms.FirstOrDefault(r => r.Name == lesson.Room);
            SelectedDiscipline = Disciplines.FirstOrDefault(d => d.ShortName == lesson.Subject);
            SelectedGroup = Groups.FirstOrDefault(g => g.Name == lesson.Group);
            SelectedTeacher = Teachers.FirstOrDefault(t => t.ShortName == lesson.Teacher);
        }

        private void ValidateCanSave()
        {
            CanSave = SelectedDay != null &&
                      SelectedTimeSlot != null &&
                      SelectedRoom != null &&
                      SelectedGroup != null &&
                      SelectedTeacher != null &&
                      SelectedDiscipline != null;
        }

        private async Task SaveAsync()
        {
            if (!CanSave)
            {
                MessageBox.Show("Заполни все обязательные поля!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var newLesson = new LessonDto
            {
                Id = _editingLesson?.Id ?? 0,
                Day = SelectedDay,
                TimeSlot = SelectedTimeSlot.Display,  // или $"{StartTime}–{EndTime}"
                Room = SelectedRoom.Name,
                Subject = SelectedDiscipline.ShortName ?? SelectedDiscipline.Name,
                Group = SelectedGroup.Name,
                Teacher = SelectedTeacher.ShortName ?? SelectedTeacher.FullName,

                // дополнительные поля, если нужны
                WeekDayNumber = GetWeekDayNumber(SelectedDay),
                StartTime = SelectedTimeSlot.StartTime,
                EndTime = SelectedTimeSlot.EndTime,
                RoomId = SelectedRoom.Id,
                TeacherId = SelectedTeacher.Id,
                GroupId = SelectedGroup.Id,
                DisciplineId = SelectedDiscipline.Id,
            };

            // Проверка конфликта
            var others = _lessons.Where(l => l != _editingLesson).ToList();
            if (_conflictChecker.HasConflict(newLesson, others))
            {
                MessageBox.Show("Конфликт!\nАудитория, преподаватель или группа уже занята в это время.",
                    "Конфликт расписания", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // Сохраняем в коллекцию главного окна
            if (_editingLesson != null)
            {
                var index = _lessons.IndexOf(_editingLesson);
                _lessons[index] = newLesson;
            }
            else
            {
                _lessons.Add(newLesson);
            }

            // Сохраняем в БД (если хочешь сразу персистить)
            // await _scheduleService.AddOrUpdateLessonAsync(newLesson);  // добавь метод

            _notifyParentUpdate?.Invoke();
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

        private static int GetWeekDayNumber(string day) => day switch
        {
            "Понедельник" => 1,
            "Вторник" => 2,
            "Среда" => 3,
            "Четверг" => 4,
            "Пятница" => 5,
            "Суббота" => 6,
            _ => 0
        };

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}