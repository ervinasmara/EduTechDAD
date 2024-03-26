using Domain.User;
using Domain.Announcement;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Domain.Class;
using Domain.Present;
using Domain.Learn.Subject;
using Domain.Learn.Study;

namespace Persistence
{
    public class DataContext : IdentityDbContext<AppUser>
    {
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // One To Many, 1 classRoom have many student
            modelBuilder.Entity<Student>()
                .HasOne(s => s.ClassRoom)
                .WithMany(c => c.Students)
                .HasForeignKey(s => s.ClassRoomId)
                .OnDelete(DeleteBehavior.Restrict);

            // One To Many, 1 Lesson have many course
            modelBuilder.Entity<Course>()
                .HasOne(s => s.Lesson)
                .WithMany(c => c.Courses)
                .HasForeignKey(s => s.LessonId)
                .OnDelete(DeleteBehavior.Restrict);

            // One To One
            modelBuilder.Entity<Attendance>()
                .HasOne(a => a.Student)
                .WithOne(s => s.Attendance)
                .HasForeignKey<Attendance>(a => a.StudentId)
                .OnDelete(DeleteBehavior.Restrict);
        }

        public DataContext(DbContextOptions options) : base(options)
        {
            // Biarkan kosong
        }

        public DbSet<Announcement> Announcements { get; set; }
        public DbSet<SuperAdmin> SuperAdmins { get; set; }
        public DbSet<Admin> Admins { get; set; }
        public DbSet<Teacher> Teachers { get; set; }
        public DbSet<Student> Students { get; set; }
        public DbSet<ClassRoom> ClassRooms { get; set; }
        public DbSet<Attendance> Attendances { get; set; }
        public DbSet<Lesson> Lessons { get; set; }
        public DbSet<Course> Courses { get; set; }
    }
}