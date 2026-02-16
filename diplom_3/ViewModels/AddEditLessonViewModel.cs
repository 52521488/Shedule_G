using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using diplom_3.Core.Interfaces;
using diplom_3.Core.Strategies;
using diplom_3.Models;

namespace diplom_3.ViewModels
{
    public class AddEditLessonViewModel : INotifyPropertyChanged
    {
        private readonly IScheduleService _scheduleService;
        private readonly IScheduleStrategy _conflictChecker;
        private readonly ObservableCollection<LessonDto> _lessons;
        private readonly Action _notifyParentUpdate;

        private LessonDto _editingLesson;

        public string WindowTitle { get; private set; } = "Добавить занятие";

        // Коллекции для ComboBox
        public ObservableCollection<string> Days { get; } = new()
        {
            "Понедельник", "Вторник", "Среда", "Четверг", "Пятница", "Суббота"
        };

        public ObservableCollection<TimeSlot> TimeSlots { get; } = new();
        public ObservableCollection<Room> Rooms { get; } = new();
        public ObservableCollection<Discipline> Disciplines { get; } = new();
        public ObservableCollection<StudentGroup> Groups { get; } = new();
        public ObservableCollection<Teacher> Teachers { get; } = new();

        // Свойства для выбранных значений
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
            set { _selectedDiscipline = value; OnPropertyChanged(); ValidateCanSave(); }
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

        // Текстовые свойства для ручного ввода
        private string _timeSlotText = "";
        public string TimeSlotText
        {
            get => _timeSlotText;
            set { _timeSlotText = value; OnPropertyChanged(); ValidateCanSave(); }
        }

        private string _roomName = "";
        public string RoomName
        {
            get => _roomName;
            set { _roomName = value; OnPropertyChanged(); ValidateCanSave(); }
        }

        private string _disciplineName = "";
        public string DisciplineName
        {
            get => _disciplineName;
            set { _disciplineName = value; OnPropertyChanged(); ValidateCanSave(); }
        }

        private string _groupName = "";
        public string GroupName
        {
            get => _groupName;
            set { _groupName = value; OnPropertyChanged(); ValidateCanSave(); }
        }

        private string _teacherName = "";
        public string TeacherName
        {
            get => _teacherName;
            set { _teacherName = value; OnPropertyChanged(); ValidateCanSave(); }
        }

        // Статус валидации
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
            _scheduleService = scheduleService;
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

            _ = LoadDataAsync();
        }

        private async Task LoadDataAsync()
        {
            try
            {
                var timeSlots = await _scheduleService.GetAllTimeSlotsAsync();
                var rooms = await _scheduleService.GetAllRoomsAsync();
                var disciplines = await _scheduleService.GetAllDisciplinesAsync();
                var groups = await _scheduleService.GetAllGroupsAsync();
                var teachers = await _scheduleService.GetAllTeachersAsync();

                if (timeSlots.Any()) { TimeSlots.Clear(); foreach (var ts in timeSlots) TimeSlots.Add(ts); }
                if (rooms.Any()) { Rooms.Clear(); foreach (var r in rooms) Rooms.Add(r); }
                if (disciplines.Any()) { Disciplines.Clear(); foreach (var d in disciplines) Disciplines.Add(d); }
                if (groups.Any()) { Groups.Clear(); foreach (var g in groups) Groups.Add(g); }
                if (teachers.Any()) { Teachers.Clear(); foreach (var t in teachers) Teachers.Add(t); }
            }
            catch
            {
                LoadTestData();
            }
        }

        private void LoadTestData()
        {
            TimeSlots.Add(new TimeSlot { Id = 1, WeekDay = 1, PairNumber = 1, StartTime = new TimeSpan(8, 0, 0), EndTime = new TimeSpan(9, 30, 0) });
            TimeSlots.Add(new TimeSlot { Id = 2, WeekDay = 1, PairNumber = 2, StartTime = new TimeSpan(9, 40, 0), EndTime = new TimeSpan(11, 10, 0) });
            TimeSlots.Add(new TimeSlot { Id = 3, WeekDay = 1, PairNumber = 3, StartTime = new TimeSpan(11, 20, 0), EndTime = new TimeSpan(12, 50, 0) });
            TimeSlots.Add(new TimeSlot { Id = 4, WeekDay = 2, PairNumber = 1, StartTime = new TimeSpan(8, 0, 0), EndTime = new TimeSpan(9, 30, 0) });
            TimeSlots.Add(new TimeSlot { Id = 5, WeekDay = 2, PairNumber = 2, StartTime = new TimeSpan(9, 40, 0), EndTime = new TimeSpan(11, 10, 0) });

            Rooms.Add(new Room { Id = 1, Name = "101", Capacity = 30 });
            Rooms.Add(new Room { Id = 2, Name = "102", Capacity = 25 });
            Rooms.Add(new Room { Id = 3, Name = "201", Capacity = 40 });
            Rooms.Add(new Room { Id = 4, Name = "301", Capacity = 35 });
            Rooms.Add(new Room { Id = 5, Name = "405", Capacity = 30 });

            Disciplines.Add(new Discipline { Id = 1, Name = "Математика", ShortName = "Матем." });
            Disciplines.Add(new Discipline { Id = 2, Name = "Программирование", ShortName = "Прогр." });
            Disciplines.Add(new Discipline { Id = 3, Name = "Базы данных", ShortName = "БД" });
            Disciplines.Add(new Discipline { Id = 4, Name = "Английский язык", ShortName = "Англ." });
            Disciplines.Add(new Discipline { Id = 5, Name = "Физика", ShortName = "Физ." });

            Groups.Add(new StudentGroup { Id = 1, Name = "ИС-21", Course = 2 });
            Groups.Add(new StudentGroup { Id = 2, Name = "ИС-22", Course = 2 });
            Groups.Add(new StudentGroup { Id = 3, Name = "ПО-21", Course = 2 });
            Groups.Add(new StudentGroup { Id = 4, Name = "КС-21", Course = 2 });

            Teachers.Add(new Teacher { Id = 1, FullName = "Иванов Иван Иванович", ShortName = "Иванов И.И." });
            Teachers.Add(new Teacher { Id = 2, FullName = "Петров Петр Петрович", ShortName = "Петров П.П." });
            Teachers.Add(new Teacher { Id = 3, FullName = "Сидоров Сидор Сидорович", ShortName = "Сидоров С.С." });
            Teachers.Add(new Teacher { Id = 4, FullName = "Смирнова Анна Михайловна", ShortName = "Смирнова А.М." });
        }

        private void LoadFromEditingLesson(LessonDto lesson)
        {
            SelectedDay = lesson.Day;
            TimeSlotText = lesson.TimeSlot;
            RoomName = lesson.Room;
            DisciplineName = lesson.Subject;
            GroupName = lesson.Group;
            TeacherName = lesson.Teacher;
        }

        private void ValidateCanSave()
        {
            // Проверяем либо выбранные значения из ComboBox, либо заполненные текстовые поля
            bool hasSelected = SelectedTimeSlot != null || !string.IsNullOrWhiteSpace(TimeSlotText);
            hasSelected = hasSelected && (SelectedRoom != null || !string.IsNullOrWhiteSpace(RoomName));
            hasSelected = hasSelected && (SelectedDiscipline != null || !string.IsNullOrWhiteSpace(DisciplineName));
            hasSelected = hasSelected && (SelectedGroup != null || !string.IsNullOrWhiteSpace(GroupName));
            hasSelected = hasSelected && (SelectedTeacher != null || !string.IsNullOrWhiteSpace(TeacherName));

            CanSave = !string.IsNullOrWhiteSpace(SelectedDay) && hasSelected;
        }

        private async Task SaveAsync()
        {
            if (!CanSave)
            {
                MessageBox.Show("Заполни все обязательные поля!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Формируем данные для сохранения (используем либо из ComboBox, либо из текстовых полей)
            string timeSlot = !string.IsNullOrWhiteSpace(TimeSlotText) ? TimeSlotText : SelectedTimeSlot?.Display ?? "";
            string room = !string.IsNullOrWhiteSpace(RoomName) ? RoomName : SelectedRoom?.Name ?? "";
            string discipline = !string.IsNullOrWhiteSpace(DisciplineName) ? DisciplineName : SelectedDiscipline?.ShortName ?? SelectedDiscipline?.Name ?? "";
            string group = !string.IsNullOrWhiteSpace(GroupName) ? GroupName : SelectedGroup?.Name ?? "";
            string teacher = !string.IsNullOrWhiteSpace(TeacherName) ? TeacherName : SelectedTeacher?.ShortName ?? SelectedTeacher?.FullName ?? "";

            var newLesson = new LessonDto
            {
                Id = _editingLesson?.Id ?? 0,
                Day = SelectedDay,
                TimeSlot = timeSlot,
                Room = room,
                Subject = discipline,
                Group = group,
                Teacher = teacher,
                WeekDayNumber = GetWeekDayNumber(SelectedDay),
            };

            // Проверка конфликта
            var others = _lessons.Where(l => l != _editingLesson).ToList();
            if (_conflictChecker.HasConflict(newLesson, others))
            {
                MessageBox.Show("Конфликт!\nАудитория, преподаватель или группа уже занята в это время.",
                    "Конфликт расписания", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // Сохраняем в коллекцию
            if (_editingLesson != null)
            {
                var index = _lessons.IndexOf(_editingLesson);
                _lessons[index] = newLesson;
            }
            else
            {
                _lessons.Add(newLesson);
            }

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
