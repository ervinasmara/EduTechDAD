using API.Services;
using Domain.Pengguna;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace API.DTOs
{
    [ApiController]
    [Route("api/[controller]")]
    public class AccountController : ControllerBase
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly TokenService _tokenService;

        public AccountController(UserManager<AppUser> userManager, TokenService tokenService)
        {
            _userManager = userManager;
            _tokenService = tokenService;
        }

        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<ActionResult<UserDto>> Login(LoginDto loginDto)
        {
            var user = await _userManager.FindByNameAsync(loginDto.Username); // Mengambil pengguna dari database

            if (user == null) Unauthorized();

            var result = await _userManager.CheckPasswordAsync(user, loginDto.Password); // Memeriksa kata sandi yang disinkronkan

            if (result)
            {
                return new UserDto // Jika hasil username dan password benar, maka inilah yang akan kita ambil dari user yang berhasil login
                {
                    Role = user.Role,
                    Username = user.UserName,
                    Token = _tokenService.CreateToken(user),
                };
            }

            return Unauthorized();
        }

        [AllowAnonymous]
        [HttpPost("register")]
        public async Task<ActionResult<UserDto>> Register(RegisterDto registerDto)
        {
            // Pemeriksaan username supaya berbeda dengan yang lain
            if (await _userManager.Users.AnyAsync(x => x.UserName == registerDto.Username))
            {
                return BadRequest("Username sudah dipakai");
            }

            var user = new AppUser
            {
                UserName = registerDto.Username,
                Role = registerDto.Role,
            };

            var result = await _userManager.CreateAsync(user, registerDto.Password);

            if (result.Succeeded)
            {
                return new UserDto
                {
                    Role = user.Role,
                    Username = user.UserName,
                    Token = _tokenService.CreateToken(user),
                };
            }
            return BadRequest(result.Errors);
        }

        [Authorize] // UNTUK GET INI HARUS LOGIN
        [HttpGet]
        public async Task<ActionResult<UserDto>> GetCurrentUser()
        {
            var user = await _userManager.FindByNameAsync(User.FindFirstValue(ClaimTypes.Name));

            return CreateUserObject(user);
        }

        private UserDto CreateUserObject(AppUser user) // KODE UNTUK MERINGKAS
        {
            return new UserDto
            {
                Role = user.Role,
                Username = user.UserName,
                Token = _tokenService.CreateToken(user),
            };
        }
    }
}