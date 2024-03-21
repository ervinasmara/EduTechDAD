using API.DTOs.Registration;
using API.Services;
using Domain.User;
using Domain.User.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Persistence;
using System.Security.Claims;

namespace API.DTOs
{
    [ApiController]
    [Route("api/[controller]")]
    public class AccountController : ControllerBase
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly TokenService _tokenService;
        private readonly DataContext _context;

        public AccountController(UserManager<AppUser> userManager, TokenService tokenService, DataContext context)
        {
            _userManager = userManager;
            _tokenService = tokenService;
            _context = context;
        }

        [AllowAnonymous]
        [HttpPost("login/admin")]
        public async Task<ActionResult<AdminDto>> LoginAdmin(LoginDto loginDto)
        {
            var user = await _userManager.FindByNameAsync(loginDto.Username);

            if (user == null) return Unauthorized();

            var result = await _userManager.CheckPasswordAsync(user, loginDto.Password);

            if (result)
            {
                try
                {
                    // Buat objek AdminDto menggunakan CreateUserObject
                    var adminDto = await CreateUserObjectAdmin(user);

                    return adminDto;
                }
                catch (Exception ex)
                {
                    // Tangani jika data admin tidak ditemukan
                    return BadRequest(ex.Message);
                }
            }
            return Unauthorized();
        }

        [AllowAnonymous]
        [HttpPost("register/admin")]
        public async Task<ActionResult<AdminDto>> RegisterAdmin(RegisterAdminDto adminDto)
        {
            // Pemeriksaan username supaya berbeda dengan yang lain
            if (await _userManager.Users.AnyAsync(x => x.UserName == adminDto.Username))
            {
                return BadRequest("Username sudah dipakai");
            }

            var user = new AppUser
            {
                UserName = adminDto.Username,
                Role = adminDto.Role,
            };

            var admin = new Admin
            {
                NameAdmin = adminDto.NameAdmin,
                AppUserId = user.Id
            };

            var result = await _userManager.CreateAsync(user, adminDto.Password);

            if (result.Succeeded)
            {
                // Simpan admin ke dalam konteks database Anda
                _context.Admins.Add(admin);
                await _context.SaveChangesAsync();

                // Gunakan metode CreateUserObjectAdmin untuk membuat objek AdminDto
                var adminDtoResult = await CreateUserObjectAdmin(user);
                return adminDtoResult; // Mengembalikan hasil dari Task<ActionResult<AdminDto>>
            }
            return BadRequest(result.Errors);
        }

        //[AllowAnonymous]
        //[HttpPost("register")]
        //public async Task<ActionResult<UserDto>> Register(RegisterDto registerDto)
        //{
        //    // Pemeriksaan username supaya berbeda dengan yang lain
        //    if (await _userManager.Users.AnyAsync(x => x.UserName == registerDto.Username))
        //    {
        //        return BadRequest("Username sudah dipakai");
        //    }

        //    var user = new AppUser
        //    {
        //        UserName = registerDto.Username,
        //        Role = registerDto.Role,
        //    };

        //    var result = await _userManager.CreateAsync(user, registerDto.Password);

        //    if (result.Succeeded)
        //    {
        //        return new UserDto
        //        {
        //            Role = user.Role,
        //            Username = user.UserName,
        //            Token = _tokenService.CreateToken(user),
        //        };
        //    }
        //    return BadRequest(result.Errors);
        //}

        //[Authorize] // UNTUK GET INI HARUS LOGIN
        //[HttpGet]
        //public async Task<ActionResult<UserDto>> GetCurrentUser()
        //{
        //    var user = await _userManager.FindByNameAsync(User.FindFirstValue(ClaimTypes.Name));

        //    return CreateUserObject(user);
        //}

        private async Task<AdminDto> CreateUserObjectAdmin(AppUser user)
        {
            // Ambil data admin terkait dari database
            var admin = await _context.Admins.FirstOrDefaultAsync(g => g.AppUserId == user.Id);

            if (admin == null)
            {
                // Handle jika data admin tidak ditemukan
                throw new Exception("Data admin tidak ditemukan");
            }

            return new AdminDto
            {
                Role = user.Role,
                Username = user.UserName,
                Token = _tokenService.CreateTokenAdmin(user, admin),
                NameAdmin = admin.NameAdmin,
            };
        }

    }
}