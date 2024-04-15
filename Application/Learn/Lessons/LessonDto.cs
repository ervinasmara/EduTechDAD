using Application.User.DTOs;

namespace Application.Learn.Lessons
{
    public class LessonDto
    {
        public string LessonName { get; set; }
        public string UniqueNumberOfLesson { get; set; }
    }

    public class LessonGetAllDto
    {
        public Guid Id { get; set; }
        public string LessonName { get; set; }
        public string UniqueNumberOfLesson { get; set; }
        public Guid TeacherId { get; set; }
        public string NameTeacher { get; set; }
    }

    public class LessonGetTeacherDto
    {
        public Guid Id { get; set; }
        public string LessonName { get; set; }
        public string UniqueNumberOfLesson { get; set; }
    }

    public class LessonCreateDto
    {
        public string LessonName { get; set; }
        public Guid TeacherId { get; set; }
    }
}
