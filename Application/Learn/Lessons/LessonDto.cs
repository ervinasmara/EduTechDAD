﻿namespace Application.Learn.Lessons
{
    public class LessonGetAllAndByIdDto
    {
        public Guid Id { get; set; }
        public string LessonName { get; set; }
        public string UniqueNumberOfLesson { get; set; }
        public string ClassName { get; set; }
        public string NameTeacher { get; set; }
        public string LessonStatus { get; set; }
    }

    public class LessonGetByTeacherIdOrClassRoomIdDto
    {
        public string LessonName { get; set; }
        public string UniqueNumberOfLesson { get; set; }
    }

    public class LessonCreateAndEditDto
    {
        public string LessonName { get; set; }
        public string ClassName { get; set; }
    }
}
