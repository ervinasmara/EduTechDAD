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
            .HasOne(l => l.ClassRoom) // Satu pelajaran memiliki satu kelas
            .WithMany(t => t.Lessons) // Satu kelas dapat dipunyai banyak pelajaran
            .HasForeignKey(l => l.ClassRoomId) // Foreign key di Lesson untuk ClassRoomId
            .OnDelete(DeleteBehavior.Restrict); // Aturan penghapusan (opsional)

        // One To Many, 1 ClassRoom have many Student
        modelBuilder.Entity<Student>()
            .HasOne(s => s.ClassRoom) // Satu siswa memiliki satu kelas
            .WithMany(c => c.Students) // Satu kelas dapat memiliki banyak siswa
            .HasForeignKey(s => s.ClassRoomId) // Mengatur foreign key di tabel Student untuk menyimpan ID kelas
            .OnDelete(DeleteBehavior.Restrict); // Menentukan aturan untuk perilaku penghapusan (opsional)

        // One To Many, 1 Lesson have many Course
        modelBuilder.Entity<Course>()
            .HasOne(s => s.Lesson) // Satu Materi memiliki satu pelajaran
            .WithMany(c => c.Courses) // Satu pelajaran dapat memiliki banyak materi
            .HasForeignKey(s => s.LessonId) // Mengatur foreign key di tabel Course untuk menyimpan ID pelajaran
            .OnDelete(DeleteBehavior.Restrict); // Menentukan aturan untuk perilaku penghapusan (opsional)

        // One To Many, 1 Course have many Assignment
        modelBuilder.Entity<Assignment>()
            .HasOne(s => s.Course) // Satu tugas memiliki satu materi
            .WithMany(c => c.Assignments) // Satu materi dapat memiliki banyak tugas
            .HasForeignKey(s => s.CourseId) // Mengatur foreign key di tabel Assignment untuk menyimpan ID materi
            .OnDelete(DeleteBehavior.Restrict); // Menentukan aturan untuk perilaku penghapusan (opsional)

        // One To Many, 1 Student have many Attendance
        modelBuilder.Entity<Attendance>()
            .HasOne(a => a.Student) // Satu kehadiran dimiliki oleh satu siswa
            .WithMany(s => s.Attendances) // Satu siswa dapat memiliki banyak kehadiran
            .HasForeignKey(a => a.StudentId) // Mengatur foreign key di tabel Attendance untuk menyimpan ID siswa
            .OnDelete(DeleteBehavior.Restrict); // Menentukan aturan untuk perilaku penghapusan (opsional)

        // One To Many, 1 Lesson have many Schedule
        modelBuilder.Entity<Schedule>()
            .HasOne(s => s.Lesson) // Satu jadwal dimiliki oleh satu pelajaran
            .WithMany(c => c.Schedules) // Satu pelajaran dapat memiliki banyak jadwal
            .HasForeignKey(s => s.LessonId) // Mengatur foreign key di tabel Schedule untuk menyimpan ID pelajaran
            .OnDelete(DeleteBehavior.Restrict); // Menentukan aturan untuk perilaku penghapusan (opsional)

        // One To Many, 1 Assignment have many AssignmentSubmission
        modelBuilder.Entity<AssignmentSubmission>()
            .HasOne(s => s.Assignment) // Satu pengumpulan tugas memiliki satu tugas
            .WithMany(a => a.AssignmentSubmissions) // Satu tugas dapat memiliki banyak pengumpulan tugas
            .HasForeignKey(s => s.AssignmentId) // Mengatur foreign key di tabel AssignmentSubmission untuk menyimpan ID tugas
            .OnDelete(DeleteBehavior.Restrict); // Menentukan aturan untuk perilaku penghapusan (opsional)

        // One To Many, 1 Student have many AssignmentSubmission
        modelBuilder.Entity<AssignmentSubmission>()
            .HasOne(z => z.Student) // Satu pengumpulan tugas dimiliki oleh satu siswa
            .WithMany(s => s.AssignmentSubmissions) // Satu siswa dapat memiliki banyak pengumpulan tugas
            .HasForeignKey(z => z.StudentId) // Mengatur foreign key di tabel AssignmentSubmission untuk menyimpan ID siswa
            .OnDelete(DeleteBehavior.Restrict); // Menentukan aturan untuk perilaku penghapusan (opsional)

        /** ======================== Many to Many ============================ **/
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