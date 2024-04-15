using Domain.Learn.Lessons;
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

        // Properti navigasi ke Lesson
        public ICollection<Lesson> Lessons { get; set; }
    }
}