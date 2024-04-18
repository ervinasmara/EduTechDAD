using Application.Learn.Lessons;

namespace Application.ClassRooms
{
    public class ClassRoomGetDto
    {
        public Guid Id { get; set; }
        public string ClassName { get; set; }
        public string UniqueNumberOfClassRoom { get; set; }
    }

    public class ClassRoomDto
    {
        public string ClassName { get; set; }
        public string UniqueNumberOfClassRoom { get; set; }
    }

    public class TeacherClassRoomDto
    {
        public string ClassName { get; set; }
        public string UniqueNumberOfClassRoom { get; set; }
        public ICollection<LessonDto> Lessons { get; set; }
    }
}
