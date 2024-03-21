using Microsoft.AspNetCore.Identity;

namespace Domain.User
{
    public class AppUser : IdentityUser
    {
        public int Role { get; set; }
        public Admin Admin { get; set; }
        public Teacher Teacher { get; set; }
        public Student Student { get; set; }
    }
}