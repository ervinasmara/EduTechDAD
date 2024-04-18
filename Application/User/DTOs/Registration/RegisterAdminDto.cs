﻿using System.ComponentModel.DataAnnotations;

namespace Application.User.DTOs.Registration
{
    public class RegisterAdminDto
    {
        public string NameAdmin { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public int Role { get; set; }
    }
}
