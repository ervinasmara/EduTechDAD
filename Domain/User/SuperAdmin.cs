using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.User
{
    public class SuperAdmin
    {
        public Guid Id { get; set; }
        public string NameSuperAdmin { get; set; }
        public int Status { get; set; } = 1;

        // Menunjukkan kunci asing ke AppUser
        public string AppUserId { get; set; }
        [ForeignKey("AppUserId")]
        public AppUser User { get; set; }
    }
}