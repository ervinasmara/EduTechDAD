using Domain.User;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Domain.Class;
using Domain.Attendances;
using Domain.Learn.Lessons;
using Domain.Learn.Courses;
using Domain.Learn.Schedules;
using Domain.Submission;
using Domain.Many_to_Many;
using Domain.Assignments;
using Domain.ToDoList;

namespace Persistence;
public class DataContext : IdentityDbContext<AppUser>
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // One To Many, 1 ClassRoom have many Lesson
        modelBuilder.Entity<Lesson>()
            .HasOne(l => l.ClassRoom)  // Satu pelajaran memiliki satu kelas
            .WithMany(t => t.Lessons)  // Satu kelas dapat dipunyai banyak pelajaran
            .HasForeignKey(l => l.ClassRoomId) // Foreign key di Lesson untuk ClassRoomId
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

        // ======================== Many to Many ============================
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
    }

    public DataContext(DbContextOptions options) : base(options)
    {
        // Biarkan kosong
    }

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
    public DbSet<TeacherLesson> TeacherLessons { get; set; }
    public DbSet<ToDoList> ToDoLists { get; set; }
}