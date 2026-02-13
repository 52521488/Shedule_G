using diplom_2.Core.Strategies;
using diplom_2.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;

namespace diplom_2
{
    public partial class MainWindow : Window
    {
        private ScheduleViewModel _vm;

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

            // Колонки: левая (время) + 7 дней
            scheduleContainer.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(100) });
            for (int i = 0; i < 7; i++)
                scheduleContainer.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

            // Строки: заголовок + по количеству слотов
            scheduleContainer.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            foreach (var slot in _vm.TimeSlots)
                scheduleContainer.RowDefinitions.Add(new RowDefinition { Height = new GridLength(90) });

            // Заголовок дней — используем ПОЛНЫЕ названия!
            AddHeaderCell(0, 0, "Время / №", true);
            string[] days = { "Понедельник", "Вторник", "Среда", "Четверг", "Пятница", "Суббота", "Воскресенье" };
            for (int d = 0; d < 7; d++)
                AddHeaderCell(0, d + 1, days[d].Substring(0, 3), true);  // можно оставить сокращённые для заголовка

            // Заполнение ячеек
            for (int row = 0; row < _vm.TimeSlots.Count; row++)
            {
                var slot = _vm.TimeSlots[row];

                // Левая колонка — слот
                AddCell(row + 1, 0, $"{row + 1}\n{slot}", isHeader: true);

                // Ячейки по дням
                for (int col = 0; col < 7; col++)
                {
                    var day = days[col];

                    // Самое важное — сравнение с Trim и IgnoreCase
                    var lesson = _vm.Lessons.FirstOrDefault(l =>
                        string.Equals(l.Day?.Trim(), day.Trim(), StringComparison.OrdinalIgnoreCase) &&
                        string.Equals(l.TimeSlot?.Trim(), slot.Trim(), StringComparison.OrdinalIgnoreCase));

                    string text = lesson != null
                        ? $"{lesson.Subject}\n{lesson.Group}\n{lesson.Teacher}"
                        : "";

                    var cell = AddCell(row + 1, col + 1, text);

                    if (lesson != null)
                    {
                        var converter = (IValueConverter)Resources["LessonBrushConv"];
                        cell.Background = (Brush)converter.Convert(lesson, typeof(Brush), null, System.Globalization.CultureInfo.CurrentCulture);
                    }
                }
            }
        }

        private Border AddCell(int row, int col, string text, bool isHeader = false)
        {
            var border = new Border
            {
                BorderBrush = Brushes.Gray,
                BorderThickness = new Thickness(1),
                Padding = new Thickness(6),
                Background = isHeader ? Brushes.LightGray : Brushes.White
            };

            var tb = new TextBlock
            {
                Text = text,
                TextAlignment = TextAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                TextWrapping = TextWrapping.Wrap
            };

            border.Child = tb;
            Grid.SetRow(border, row);
            Grid.SetColumn(border, col);
            scheduleContainer.Children.Add(border);

            return border;
        }

        private void AddHeaderCell(int row, int col, string text, bool bold = false)
        {
            var cell = AddCell(row, col, text, true);
            if (bold)
            {
                ((TextBlock)cell.Child).FontWeight = FontWeights.Bold;
            }
        }

        private void zoomSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            // Зум обновляется автоматически через binding + converter
        }

        // Добавление нового занятия
        private void AddLesson_Click(object sender, RoutedEventArgs e)
        {
            var window = new AddEditLessonWindow();
            window.DataContext = new AddLessonViewModel(
                lessons: _vm.Lessons,
                notifyUpdate: () => BuildScheduleGrid(),
                conflictChecker: new SimpleConflictStrategy()
            );
            window.ShowDialog();
        }

        // Редактирование (вызывается из двойного клика по ячейке)
        private void EditLesson(LessonModel lesson)
        {
            var window = new AddEditLessonWindow { Title = "Редактировать занятие" };
            window.DataContext = new AddLessonViewModel(
                lessons: _vm.Lessons,
                notifyUpdate: () => BuildScheduleGrid(),
                conflictChecker: new SimpleConflictStrategy(),
                editingLesson: lesson
            );
            window.ShowDialog();
        }
        private void Refresh_Click(object sender, RoutedEventArgs e)
        {
            BuildScheduleGrid();
        }
    }
}