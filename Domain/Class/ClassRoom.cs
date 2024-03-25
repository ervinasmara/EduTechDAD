using Domain.User;

namespace Domain.Class
{
    public class ClassRoom
    {
        public Guid Id { get; set; }
        public string ClassName { get; set; }
        public string UniqueNumber { get; set; }
        public ICollection<Student> Students { get; set; }
    }
}