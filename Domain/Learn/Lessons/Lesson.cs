﻿using Domain.Learn.Schedules;
using Domain.Learn.Courses;
using Domain.User;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Learn.Lessons
{
    public class Lesson
    {
        public Guid Id { get; set; }
        public string LessonName { get; set; }
        public string UniqueNumberOfLesson { get; set; }

        // Properti navigasi ke Teacher (Guru)
        public Guid TeacherId { get; set; }
        [ForeignKey("TeacherId")]
        public Teacher Teacher { get; set; }

        // Properti navigasi ke Course
        public ICollection<Course> Courses { get; set; }

        // Relasi dengan jadwal
        public ICollection<Schedule> Schedules { get; set; } = new List<Schedule>();
    }
}