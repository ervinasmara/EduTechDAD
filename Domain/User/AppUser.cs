using Microsoft.AspNetCore.Identity;

namespace Domain.User
{
    public class AppUser : IdentityUser
    {
        public int Role { get; set; }
        /*
         * 1 = Admin
         * 2 = Teacher
         * 3 = Student
         * 4 = Superadmin
        */

        public SuperAdmin SuperAdmin { get; set; }
        public Admin Admin { get; set; }
        public Teacher Teacher { get; set; }
        public Student Student { get; set; }
    }
}