using Domain.User;
using Domain.Announcement;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Domain.Class;
using Domain.Present;
using Domain.Learn.Subject;
using Domain.Learn.Study;
using Domain.Task;
using Domain.Learn.Agenda;
using Domain.Submission;
using Domain.InfoRecaps;

namespace Persistence
{
    public class DataContext : IdentityDbContext<AppUser>
    {
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Lesson>()
                .HasOne(l => l.Teacher)  // Satu pelajaran memiliki satu guru
                .WithMany(t => t.Lessons)  // Satu guru dapat mengajar banyak pelajaran
                .HasForeignKey(l => l.TeacherId) // Foreign key di Lesson untuk TeacherId
                .OnDelete(DeleteBehavior.Restrict); // Aturan penghapusan (opsional)


            // One To Many, 1 ClassRoom have many Student
            modelBuilder.Entity<Student>()
                .HasOne(s => s.ClassRoom)
                .WithMany(c => c.Students)
                .HasForeignKey(s => s.ClassRoomId)
                .OnDelete(DeleteBehavior.Restrict);

            // One To Many, 1 Lesson have many Course
            modelBuilder.Entity<Course>()
                .HasOne(s => s.Lesson)
                .WithMany(c => c.Courses)
                .HasForeignKey(s => s.LessonId)
                .OnDelete(DeleteBehavior.Restrict);

            // One To Many, 1 Course have many Assignment
            modelBuilder.Entity<Assignment>()
                .HasOne(s => s.Course)
                .WithMany(c => c.Assignments)
                .HasForeignKey(s => s.CourseId)
                .OnDelete(DeleteBehavior.Restrict);

            // One To Many, 1 Student have many Attendance
            modelBuilder.Entity<Attendance>()
                .HasOne(a => a.Student)
                .WithMany(s => s.Attendances)
                .HasForeignKey(a => a.StudentId)
                .OnDelete(DeleteBehavior.Restrict);

            // One To Many, 1 ClassRoom have many Schedule
            modelBuilder.Entity<Schedule>()
                .HasOne(s => s.ClassRoom)
                .WithMany(c => c.Schedules)
                .HasForeignKey(s => s.ClassRoomId)
                .OnDelete(DeleteBehavior.Restrict);

            // One To Many, 1 Lesson have many Schedule
            modelBuilder.Entity<Schedule>()
                .HasOne(s => s.Lesson)
                .WithMany(c => c.Schedules)
                .HasForeignKey(s => s.LessonId)
                .OnDelete(DeleteBehavior.Restrict);

            // One To Many, 1 Assignment have many AssignmentSubmission
            modelBuilder.Entity<AssignmentSubmission>()
                .HasOne(s => s.Assignment)
                .WithMany(a => a.AssignmentSubmissions)
                .HasForeignKey(s => s.AssignmentId)
                .OnDelete(DeleteBehavior.Restrict);

            // One To Many, 1 Student have many AssignmentSubmission
            modelBuilder.Entity<AssignmentSubmission>()
                .HasOne(z => z.Student)
                .WithMany(s => s.AssignmentSubmissions)
                .HasForeignKey(z => z.StudentId)
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
        public DbSet<Assignment> Assignments { get; set; }
        public DbSet<Schedule> Schedules { get; set; }
        public DbSet<AssignmentSubmission> AssignmentSubmissions { get; set; }
        public DbSet<InfoRecap> InfoRecaps { get; set; }
    }
}