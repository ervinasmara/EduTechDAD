using Domain.User;
using Domain.Announcement;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Domain.Class;
using Domain.Attendances;
using Domain.Learn.Lessons;
using Domain.Learn.Courses;
using Domain.Learn.Schedules;
using Domain.Submission;
using Domain.InfoRecaps;
using Domain.Many_to_Many;
using Domain.Assignments;

namespace Persistence
{
    public class DataContext : IdentityDbContext<AppUser>
    {
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            //modelBuilder.Entity<Lesson>()
            //    .HasOne(l => l.Teacher)  // Satu pelajaran memiliki satu guru
            //    .WithMany(t => t.Lessons)  // Satu guru dapat mengajar banyak pelajaran
            //    .HasForeignKey(l => l.TeacherId) // Foreign key di Lesson untuk TeacherId
            //    .OnDelete(DeleteBehavior.Restrict); // Aturan penghapusan (opsional)

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

            // Menentukan kunci utama untuk entitas perantara TeacherCourse
            modelBuilder.Entity<TeacherCourse>()
                .HasKey(tc => new { tc.TeacherId, tc.CourseId });

            // Menentukan hubungan antara TeacherCourse dan Teacher
            modelBuilder.Entity<TeacherCourse>()
                .HasOne(tc => tc.Teacher) // TeacherCourse memiliki satu Teacher
                .WithMany(t => t.TeacherCourses) // Teacher memiliki banyak TeacherCourse
                .HasForeignKey(tc => tc.TeacherId); // Kunci asing TeacherCourse adalah TeacherId

            // Menentukan hubungan antara TeacherCourse dan Course
            modelBuilder.Entity<TeacherCourse>()
                .HasOne(tc => tc.Course) // TeacherCourse memiliki satu Course
                .WithMany(c => c.TeacherCourses) // Course memiliki banyak TeacherCourse
                .HasForeignKey(tc => tc.CourseId); // Kunci asing TeacherCourse adalah CourseId

            // ======================== Many to Many ============================
            // Konfigurasi many-to-many relasi antara Course dan ClassRoom
            modelBuilder.Entity<CourseClassRoom>()
                .HasKey(ccr => new { ccr.CourseId, ccr.ClassRoomId });

            // CourseClassRoom memiliki relasi one-to-many ke Course
            modelBuilder.Entity<CourseClassRoom>()
                .HasOne(ccr => ccr.Course) // Setiap CourseClassRoom memiliki satu Course
                .WithMany(course => course.CourseClassRooms) // Setiap Course memiliki banyak CourseClassRoom
                .HasForeignKey(ccr => ccr.CourseId); // ForeignKey CourseId di CourseClassRoom

            // CourseClassRoom memiliki relasi one-to-many ke ClassRoom
            modelBuilder.Entity<CourseClassRoom>()
                .HasOne(ccr => ccr.ClassRoom) // Setiap CourseClassRoom memiliki satu ClassRoom
                .WithMany(classRoom => classRoom.CourseClassRooms) // Setiap ClassRoom memiliki banyak CourseClassRoom
                .HasForeignKey(ccr => ccr.ClassRoomId); // ForeignKey ClassRoomId di CourseClassRoom


            // Konfigurasi many-to-many relasi antara Assignment dan ClassRoom
            modelBuilder.Entity<AssignmentClassRoom>()
                .HasKey(ccr => new { ccr.AssignmentId, ccr.ClassRoomId });

            // AssignmentClassRoom memiliki relasi one-to-many ke Assignment
            modelBuilder.Entity<AssignmentClassRoom>()
                .HasOne(ccr => ccr.Assignment) // Setiap AssignmentClassRoom memiliki satu Assignment
                .WithMany(course => course.AssignmentClassRooms) // Setiap Assignment memiliki banyak AssignmentClassRoom
                .HasForeignKey(ccr => ccr.AssignmentId); // ForeignKey AssignmentId di AssignmentClassRoom

            // AssignmentClassRoom memiliki relasi one-to-many ke ClassRoom
            modelBuilder.Entity<AssignmentClassRoom>()
                .HasOne(ccr => ccr.ClassRoom) // Setiap AssignmentClassRoom memiliki satu ClassRoom
                .WithMany(classRoom => classRoom.AssignmentClassRooms) // Setiap ClassRoom memiliki banyak AssignmentClassRoom
                .HasForeignKey(ccr => ccr.ClassRoomId); // ForeignKey ClassRoomId di AssignmentClassRoom


            // Konfigurasi many-to-many relasi antara Teacher dan Lesson
            modelBuilder.Entity<TeacherLesson>()
                .HasKey(ccr => new { ccr.TeacherId, ccr.LessonId });

            // TeacherLesson memiliki relasi one-to-many ke Teacher
            modelBuilder.Entity<TeacherLesson>()
                .HasOne(ccr => ccr.Teacher) // Setiap TeacherLesson memiliki satu Teacher
                .WithMany(course => course.TeacherLessons) // Setiap Teacher memiliki banyak TeacherLesson
                .HasForeignKey(ccr => ccr.TeacherId); // ForeignKey TeacherId di TeacherLesson

            // TeacherLesson memiliki relasi one-to-many ke Lesson
            modelBuilder.Entity<TeacherLesson>()
                .HasOne(ccr => ccr.Lesson) // Setiap TeacherLesson memiliki satu Lesson
                .WithMany(classRoom => classRoom.TeacherLessons) // Setiap Lesson memiliki banyak TeacherLesson
                .HasForeignKey(ccr => ccr.LessonId); // ForeignKey LessonId di TeacherLesson


            // Konfigurasi many-to-many relasi antara Teacher dan ClassRoom
            modelBuilder.Entity<TeacherClassRoom>()
                .HasKey(ccr => new { ccr.TeacherId, ccr.ClassRoomId });

            // TeacherClassRoom memiliki relasi one-to-many ke Teacher
            modelBuilder.Entity<TeacherClassRoom>()
                .HasOne(ccr => ccr.Teacher) // Setiap TeacherClassRoom memiliki satu Teacher
                .WithMany(course => course.TeacherClassRooms) // Setiap Teacher memiliki banyak TeacherClassRoom
                .HasForeignKey(ccr => ccr.TeacherId); // ForeignKey TeacherId di TeacherClassRoom

            // TeacherClassRoom memiliki relasi one-to-many ke ClassRoom
            modelBuilder.Entity<TeacherClassRoom>()
                .HasOne(ccr => ccr.ClassRoom) // Setiap TeacherClassRoom memiliki satu ClassRoom
                .WithMany(classRoom => classRoom.TeacherClassRooms) // Setiap ClassRoom memiliki banyak TeacherClassRoom
                .HasForeignKey(ccr => ccr.ClassRoomId); // ForeignKey ClassRoomId di TeacherClassRoom
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
        public DbSet<CourseClassRoom> CourseClassRooms { get; set; }
        public DbSet<AssignmentClassRoom> AssignmentClassRooms { get; set; }
        public DbSet<TeacherLesson> TeacherLessons { get; set; }
        public DbSet<TeacherClassRoom> TeacherClassRooms { get; set; }
        public DbSet<TeacherCourse> TeacherCourses { get; set; }
    }
}