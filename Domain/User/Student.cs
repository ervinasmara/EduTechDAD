using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.User
{
    public class Student
    {
        public Guid Id { get; set; }
        public string NameStudent { get; set; }
        public DateOnly BirthDate { get; set; }
        public string BirthPlace { get; set; }
        public string Address { get; set; }
        public string PhoneNumber { get; set; }
        public string Nis { get; set; }
        public string ParentName { get; set; }
        public int Gender { get; set; }

        // Menunjukkan kunci asing ke AppUser
        public string AppUserId { get; set; }
        [ForeignKey("AppUserId")]
        public AppUser User { get; set; }
    }
}