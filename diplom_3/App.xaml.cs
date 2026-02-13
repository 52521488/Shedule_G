using Microsoft.Extensions.DependencyInjection;
using diplom_3.Core;
using diplom_3.Core.Interfaces;
using diplom_3.Core.Services;
using Microsoft.EntityFrameworkCore;
using System.Windows;

namespace diplom_3
{
    public partial class App : Application
    {
        public IServiceProvider ServiceProvider { get; private set; }

        public App()
        {
            var services = new ServiceCollection();

            // строка подключения — подставь свою
            string connString = @"Server=.\SQLEXPRESS;Database=CollegeSchedule;Trusted_Connection=True;TrustServerCertificate=True;";

            services.AddDbContext<CollegeScheduleContext>(options =>
                options.UseSqlServer(connString));

            services.AddScoped<IScheduleService, ScheduleService>();

            // регистрируем окна/VM если нужно
            services.AddTransient<MainWindow>();
            services.AddTransient<AddEditLessonWindow>();

            ServiceProvider = services.BuildServiceProvider();
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            var mainWindow = ServiceProvider.GetRequiredService<MainWindow>();
            mainWindow.Show();
        }
    }
}