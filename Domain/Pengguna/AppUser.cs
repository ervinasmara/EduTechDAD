using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Pengguna
{
    public class AppUser : IdentityUser
    {
        public int Role { get; set; }
    }
}