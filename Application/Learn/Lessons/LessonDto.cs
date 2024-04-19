using Application.ClassRooms;

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
        public ICollection<string> ClassNames { get; set; }
        public ICollection<TeacherLessonGetAllDto> TeacherLesson { get; set; }
    }

    public class TeacherLessonGetAllDto
    {
        public string NameTeacher { get; set; }
        public ICollection<string> ClassNames { get; set; }
    }

    public class LessonGetTeacherDto
    {
        public Guid Id { get; set; }
        public string LessonName { get; set; }
        public string UniqueNumberOfLesson { get; set; }
    }

    public class LessonTeacherIdGetDto
    {
        public string LessonName { get; set; }
        public ICollection<ClassRoomDto> ClassRooms { get; set; }
    }

    public class LessonCreateDto
    {
        public string LessonName { get; set; }
        public ICollection<string> ClassNames { get; set; }
    }
}
