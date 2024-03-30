﻿using Domain.User;
using Domain.Announcement;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Domain.Class;
using Domain.Present;
using Domain.Learn.Subject;
using Domain.Learn.Study;
using Domain.Task;

namespace Persistence
{
    public class DataContext : IdentityDbContext<AppUser>
    {
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

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
    }
}