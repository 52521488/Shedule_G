using diplom_3.Core.Strategies;
using diplom_3.Core.Interfaces;
using diplom_3.Models;
using diplom_3.ViewModels;
using diplom_3.Core;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using Microsoft.Extensions.DependencyInjection;

namespace diplom_3
{
    public partial class MainWindow : Window
    {
        private ScheduleViewModel _vm;
        private readonly string[] _days = { "Понедельник", "Вторник", "Среда", "Четверг", "Пятница", "Суббота" };
        
        // Время и номер пары
        private readonly (string time, int pairNo)[] _timeSlots = 
        {
            ("08:00-09:30", 1),
            ("09:40-11:10", 2),
            ("11:20-12:50", 3),
            ("13:20-14:50", 4),
            ("15:00-16:30", 5),
            ("16:40-18:10", 6)
        };

        public MainWindow()
        {
            InitializeComponent();
            _vm = new ScheduleViewModel();
            DataContext = _vm;

            Loaded += (s, e) => BuildScheduleGrid();
        }

        private void BuildScheduleGrid()
        {
            scheduleContainer.Children.Clear();
            scheduleContainer.RowDefinitions.Clear();
            scheduleContainer.ColumnDefinitions.Clear();

            // Получаем уникальные группы
            var groups = _vm.Lessons
                .Select(l => l.Group)
                .Distinct()
                .OrderBy(g => g)
                .ToList();
            
            // Если нет групп - добавляем тестовые
            if (groups.Count == 0)
            {
                groups = new List<string> { "Группа 1", "Группа 2", "Группа 3" };
            }

            // Колонки: левая (время) + колонки для каждой группы × 6 дней
            // Структура: время | Группа1_Пн | Группа1_Вт | ... | Группа2_Пн | ...
            scheduleContainer.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(100) }); // время
            
            // Заголовки групп - объединяем по 6 дней (понедельник-суббота)
            foreach (var group in groups)
            {
                // Для каждой группы создаем 6 колонок (по дню)
                for (int d = 0; d < 6; d++)
                {
                    scheduleContainer.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                }
            }

            // Строки: заголовок + по количеству слотов
            scheduleContainer.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto }); // заголовок дней
            foreach (var slot in _timeSlots)
                scheduleContainer.RowDefinitions.Add(new RowDefinition { Height = new GridLength(80) });

            // === ЗАГОЛОВОК ===
            // Первая ячейка - пустая или "Время/День"
            AddHeaderCell(0, 0, "Время", true);

            // Заголовки дней для каждой группы
            int col = 1;
            foreach (var group in groups)
            {
                foreach (var day in _days)
                {
                    AddHeaderCell(0, col, $"{group}\n{day}", true);
                    col++;
                }
            }

            // === ЯЧЕЙКИ РАСПИСАНИЯ ===
            for (int row = 0; row < _timeSlots.Length; row++)
            {
                var (time, pairNo) = _timeSlots[row];

                // Левая колонка — время + номер пары
                AddTimeCell(row + 1, 0, time, pairNo);

                // Ячейки по группам и дням
                col = 1;
                foreach (var group in groups)
                {
                    foreach (var day in _days)
                    {
                        // Находим занятия для этой группы, дня и времени
                        var lessons = _vm.Lessons.Where(l =>
                            string.Equals(l.Group?.Trim(), group.Trim(), StringComparison.OrdinalIgnoreCase) &&
                            string.Equals(l.Day?.Trim(), day.Trim(), StringComparison.OrdinalIgnoreCase) &&
                            l.PairNumber == pairNo)
                            .ToList();

                        AddScheduleCell(row + 1, col, lessons);
                        col++;
                    }
                }
            }
        }

        private void AddHeaderCell(int row, int col, string text, bool isHeader = false)
        {
            var border = new Border
            {
                BorderBrush = Brushes.DarkGray,
                BorderThickness = new Thickness(1),
                Padding = new Thickness(4),
                Background = isHeader ? Brushes.LightSteelBlue : Brushes.White,
                MinHeight = 40
            };

            var tb = new TextBlock
            {
                Text = text,
                TextAlignment = TextAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                TextWrapping = TextWrapping.Wrap,
                FontSize = 11,
                FontWeight = isHeader ? FontWeights.Bold : FontWeights.Normal
            };

            border.Child = tb;
            Grid.SetRow(border, row);
            Grid.SetColumn(border, col);
            scheduleContainer.Children.Add(border);
        }

        private void AddTimeCell(int row, int col, string time, int pairNo)
        {
            var border = new Border
            {
                BorderBrush = Brushes.DarkGray,
                BorderThickness = new Thickness(1),
                Padding = new Thickness(4),
                Background = new SolidColorBrush(Color.FromRgb(240, 245, 250))
            };

            var panel = new StackPanel 
            { 
                Orientation = Orientation.Vertical,
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center
            };

            // Номер пары
            panel.Children.Add(new TextBlock 
            { 
                Text = pairNo.ToString(), 
                FontSize = 14, 
                FontWeight = FontWeights.Bold,
                HorizontalAlignment = HorizontalAlignment.Center,
                Foreground = Brushes.DarkBlue
            });

            // Время
            panel.Children.Add(new TextBlock 
            { 
                Text = time, 
                FontSize = 10,
                HorizontalAlignment = HorizontalAlignment.Center,
                Foreground = Brushes.Gray
            });

            border.Child = panel;
            Grid.SetRow(border, row);
            Grid.SetColumn(border, col);
            scheduleContainer.Children.Add(border);
        }

        private void AddScheduleCell(int row, int col, List<LessonDto> lessons)
        {
            var border = new Border
            {
                BorderBrush = Brushes.DarkGray,
                BorderThickness = new Thickness(1),
                Background = lessons.Any() ? new SolidColorBrush(Color.FromRgb(250, 252, 255)) : Brushes.White
            };

            // Если есть занятия - создаем сетку 2x2 (верх/низ неделя × подгруппы)
            if (lessons.Any())
            {
                var grid = new Grid();
                
                // 2 строки: верхняя неделя, нижняя неделя
                grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
                grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
                
                // 2 колонки: подгруппа 1, подгруппа 2 (или пусто)
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

                // Заполняем ячейки
                // Верхняя неделя
                var upperLessons = lessons.Where(l => l.WeekType == "both" || l.WeekType == "upper").ToList();
                var lowerLessons = lessons.Where(l => l.WeekType == "both" || l.WeekType == "lower").ToList();

                // Ячейка: Верх + Подгруппа 1
                var cellUpper1 = CreateLessonCell(upperLessons, "upper", "1");
                Grid.SetRow(cellUpper1, 0);
                Grid.SetColumn(cellUpper1, 0);
                grid.Children.Add(cellUpper1);

                // Ячейка: Верх + Подгруппа 2
                var cellUpper2 = CreateLessonCell(upperLessons, "upper", "2");
                Grid.SetRow(cellUpper2, 0);
                Grid.SetColumn(cellUpper2, 1);
                grid.Children.Add(cellUpper2);

                // Ячейка: Низ + Подгруппа 1
                var cellLower1 = CreateLessonCell(lowerLessons, "lower", "1");
                Grid.SetRow(cellLower1, 1);
                Grid.SetColumn(cellLower1, 0);
                grid.Children.Add(cellLower1);

                // Ячейка: Низ + Подгруппа 2
                var cellLower2 = CreateLessonCell(lowerLessons, "lower", "2");
                Grid.SetRow(cellLower2, 1);
                Grid.SetColumn(cellLower2, 1);
                grid.Children.Add(cellLower2);

                border.Child = grid;
            }

            Grid.SetRow(border, row);
            Grid.SetColumn(border, col);
            scheduleContainer.Children.Add(border);
        }

        private Border CreateLessonCell(List<LessonDto> lessons, string weekType, string subGroupNo)
        {
            // Фильтруем по подгруппе
            var filtered = lessons.Where(l => 
                (string.IsNullOrEmpty(l.SubGroupName) && subGroupNo == "1") ||
                l.SubGroupName == subGroupNo ||
                (l.SubGroupName?.Contains(subGroupNo) == true))
                .ToList();

            var border = new Border
            {
                BorderBrush = Brushes.LightGray,
                BorderThickness = new Thickness(0.5),
                Padding = new Thickness(2),
                Background = filtered.Any() 
                    ? new SolidColorBrush(Color.FromRgb(220, 235, 250)) 
                    : Brushes.Transparent,
                Tag = filtered.FirstOrDefault() // для клика
            };

            if (filtered.Any())
            {
                var panel = new StackPanel { Orientation = Orientation.Vertical };

                // Заголовок: Верх/Низ + подгруппа
                var headerText = weekType == "upper" ? "Верх" : "Низ";
                if (!string.IsNullOrEmpty(filtered.First().SubGroupName))
                {
                    headerText += $" ({filtered.First().SubGroupName})";
                }
                
                panel.Children.Add(new TextBlock
                {
                    Text = headerText,
                    FontSize = 8,
                    FontWeight = FontWeights.Bold,
                    Foreground = Brushes.DarkBlue,
                    Margin = new Thickness(0, 0, 0, 1)
                });

                // Предмет
                panel.Children.Add(new TextBlock
                {
                    Text = filtered.First().Subject,
                    FontSize = 9,
                    FontWeight = FontWeights.SemiBold,
                    TextWrapping = TextWrapping.Wrap
                });

                // Преподаватель + аудитория
                panel.Children.Add(new TextBlock
                {
                    Text = $"{filtered.First().Teacher} • {filtered.First().Room}",
                    FontSize = 8,
                    Foreground = Brushes.Gray,
                    TextWrapping = TextWrapping.Wrap
                });

                border.Child = panel;

                // Двойной клик для редактирования
                border.MouseLeftButtonDown += (s, e) =>
                {
                    if (e.ClickCount == 2 && filtered.Any())
                    {
                        EditLesson(filtered.First());
                    }
                };
            }

            return border;
        }

        // Добавление
        private void AddLesson_Click(object sender, RoutedEventArgs e)
        {
            var window = new AddEditLessonWindow();
            var service = (Application.Current as App)?.ServiceProvider.GetRequiredService<IScheduleService>();

            window.DataContext = new AddEditLessonViewModel(
                service,
                _vm.Lessons,
                () => BuildScheduleGrid(),
                new SimpleConflictStrategy()
            );

            window.ShowDialog();
        }

        // Редактирование
        private void EditLesson(LessonDto lesson)
        {
            var window = new AddEditLessonWindow { Title = "Редактировать занятие" };
            var service = (Application.Current as App)?.ServiceProvider.GetRequiredService<IScheduleService>();

            window.DataContext = new AddEditLessonViewModel(
                service,
                _vm.Lessons,
                () => BuildScheduleGrid(),
                new SimpleConflictStrategy(),
                lesson
            );

            window.ShowDialog();
        }

        private void Refresh_Click(object sender, RoutedEventArgs e)
        {
            BuildScheduleGrid();
        }

        private void zoomSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            // Зум обновляется автоматически через binding + converter
        }
    }
}
