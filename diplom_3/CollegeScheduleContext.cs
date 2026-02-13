using Microsoft.EntityFrameworkCore;
using diplom_3.Models;

namespace diplom_3.Core
{
    public class CollegeScheduleContext : DbContext
    {
        public DbSet<StudentGroup> StudentGroups { get; set; } = null!;
        public DbSet<Discipline> Disciplines { get; set; } = null!;
        public DbSet<Teacher> Teachers { get; set; } = null!;
        public DbSet<Room> Rooms { get; set; } = null!;
        public DbSet<TimeSlot> TimeSlots { get; set; } = null!;
        public DbSet<ScheduleEntry> ScheduleEntries { get; set; } = null!;

        // остальные DbSet подключай позже, когда понадобятся

        public CollegeScheduleContext(DbContextOptions<CollegeScheduleContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ScheduleEntry>()
                .HasOne(e => e.TimeSlot)
                .WithMany()
                .HasForeignKey(e => e.TimeSlotId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ScheduleEntry>()
                .HasOne(e => e.Teacher)
                .WithMany()
                .HasForeignKey(e => e.TeacherId)
                .OnDelete(DeleteBehavior.Restrict);

            // Добавляй остальные связи по мере необходимости
            // cascade delete почти всегда опасен в расписаниях
        }
    }
}