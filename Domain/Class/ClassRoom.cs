using Domain.Learn.Lessons;
using Domain.User;

namespace Domain.Class;
public class ClassRoom
{
    public Guid Id { get; set; }
    public string ClassName { get; set; }
    public string LongClassName { get; set; }
    public string UniqueNumberOfClassRoom { get; set; }
    public int Status { get; set; }

    // Relasi dengan mapel
    public ICollection<Lesson> Lessons { get; set; }

    // Relasi dengan siswa
    public ICollection<Student> Students { get; set; }
}