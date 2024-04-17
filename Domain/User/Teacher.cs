﻿using Domain.Learn.Courses;
using Domain.Many_to_Many;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.User
{
    public class Teacher
    {
        public Guid Id { get; set; }
        public string NameTeacher { get; set; }
        public DateOnly BirthDate { get; set; }
        public string BirthPlace { get; set; }
        public string Address { get; set; }
        public string PhoneNumber { get; set; }
        public string Nip { get; set; }

        // Menunjukkan kunci asing ke AppUser
        public string AppUserId { get; set; }
        [ForeignKey("AppUserId")]
        public AppUser User { get; set; }

        // Relasi many-to-many dengan Lesson melalui tabel pivot TeacherLesson
        public ICollection<TeacherLesson> TeacherLessons { get; set; }

        // Relasi many-to-many dengan ClassRoom melalui tabel pivot TeacherClassRoom
        public ICollection<TeacherClassRoom> TeacherClassRooms { get; set; }

        // Properti navigasi ke TeacherCourse
        public ICollection<TeacherCourse> TeacherCourses { get; set; }
    }
}