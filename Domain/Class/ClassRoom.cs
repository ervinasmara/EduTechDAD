using Domain.Learn.Agenda;
using Domain.User;

namespace Domain.Class
{
    public class ClassRoom
    {
        public Guid Id { get; set; }
        public string ClassName { get; set; }
        public string UniqueNumberOfClassRoom { get; set; }

        // Relasi dengan siswa
        public ICollection<Student> Students { get; set; }

        // Relasi dengan jadwal
        public ICollection<Schedule> Schedules { get; set; }
    }
}