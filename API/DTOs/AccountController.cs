using API.Controllers;
using API.DTOs.Registration;
using API.Services;
using Domain.User;
using Domain.User.DTOs;
using API.DTOs.Edit;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Persistence;
using System.Security.Claims;
using Application.User.Student;

namespace API.DTOs
{
    [ApiController]
    [Route("api/[controller]")]
    public class AccountController : BaseApiController
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

        // =========================== GET DATA =========================== //
        [HttpGet("students"), AllowAnonymous]
        public async Task<IActionResult> GetStudents(CancellationToken ct)
        {
            return HandleResult(await Mediator.Send(new ListStudent.Query(), ct));
        }

        [HttpGet("student/{id}"), AllowAnonymous]
        public async Task<ActionResult> GetStudentById(Guid id, CancellationToken ct)
        {
            return HandleResult(await Mediator.Send(new DetailsStudent.Query { Id = id }, ct));
        }
        // =========================== LOGIN =========================== //
        [AllowAnonymous]
        [HttpPost("login/superadmin")]
        public async Task<ActionResult<SuperAdminDto>> LoginSuperAdmin(LoginDto loginDto)
        {
            var user = await _userManager.FindByNameAsync(loginDto.Username);

            if (user == null) return Unauthorized();

            var result = await _userManager.CheckPasswordAsync(user, loginDto.Password);

            if (result)
            {
                try
                {
                    // Buat objek SuperAdminDto menggunakan CreateUserObject
                    var superAdminDto = await CreateUserObjectSuperAdmin(user);

                    return superAdminDto;
                }
                catch (Exception ex)
                {
                    // Tangani jika data superAdmin tidak ditemukan
                    return BadRequest(ex.Message);
                }
            }
            return Unauthorized();
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
        [HttpPost("login/teacher")]
        public async Task<ActionResult<TeacherDto>> LoginTeacher(LoginDto loginDto)
        {
            var user = await _userManager.FindByNameAsync(loginDto.Username);

            if (user == null) return Unauthorized();

            var result = await _userManager.CheckPasswordAsync(user, loginDto.Password);

            if (result)
            {
                try
                {
                    // Buat objek TeacherDto menggunakan CreateUserObject
                    var teacherDto = await CreateUserObjectTeacher(user);

                    return teacherDto;
                }
                catch (Exception ex)
                {
                    // Tangani jika data teacher tidak ditemukan
                    return BadRequest(ex.Message);
                }
            }
            return Unauthorized();
        }

        [AllowAnonymous]
        [HttpPost("login/student")]
        public async Task<ActionResult<StudentDto>> LoginStudent(LoginDto loginDto)
        {
            var user = await _userManager.FindByNameAsync(loginDto.Username);

            if (user == null) return Unauthorized();

            var result = await _userManager.CheckPasswordAsync(user, loginDto.Password);

            if (result)
            {
                try
                {
                    // Buat objek StudentDto menggunakan CreateUserObject
                    var studentDto = await CreateUserObjectStudent(user);

                    return studentDto;
                }
                catch (Exception ex)
                {
                    // Tangani jika data student tidak ditemukan
                    return BadRequest(ex.Message);
                }
            }
            return Unauthorized();
        }

        // =========================== REGISTER =========================== //
        [AllowAnonymous]
        [HttpPost("register/superAdmin")]
        public async Task<ActionResult<SuperAdminGetDto>> RegisterSuperAdmin(RegisterSuperAdminDto superAdminDto)
        {
            // Pemeriksaan username supaya berbeda dengan yang lain
            if (await _userManager.Users.AnyAsync(x => x.UserName == superAdminDto.Username))
            {
                return BadRequest("Username already in use");
            }

            var user = new AppUser
            {
                UserName = superAdminDto.Username,
                Role = superAdminDto.Role,
            };

            var superAdmin = new SuperAdmin
            {
                NameSuperAdmin = superAdminDto.NameSuperAdmin,
                AppUserId = user.Id
            };

            var result = await _userManager.CreateAsync(user, superAdminDto.Password);

            if (result.Succeeded)
            {
                // Simpan superAdmin ke dalam konteks database Anda
                _context.SuperAdmins.Add(superAdmin);
                await _context.SaveChangesAsync();

                // Gunakan metode CreateUserObjectSuperAdmin untuk membuat objek SuperAdminDto
                var superAdminDtoResult = await CreateUserObjectSuperAdminGet(user);
                return superAdminDtoResult; // Mengembalikan hasil dari Task<ActionResult<SuperAdminDto>>
            }
            return BadRequest(result.Errors);
        }

        [AllowAnonymous]
        [HttpPost("register/admin")]
        public async Task<ActionResult<AdminGetDto>> RegisterAdmin(RegisterAdminDto adminDto)
        {
            // Pemeriksaan username supaya berbeda dengan yang lain
            if (await _userManager.Users.AnyAsync(x => x.UserName == adminDto.Username))
            {
                return BadRequest("Username already in use");
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
                var adminDtoResult = await CreateUserObjectAdminGet(user);
                return adminDtoResult; // Mengembalikan hasil dari Task<ActionResult<AdminDto>>
            }
            return BadRequest(result.Errors);
        }

        [AllowAnonymous]
        [HttpPost("register/teacher")]
        public async Task<ActionResult<TeacherGetDto>> RegisterTeacher(RegisterTeacherDto teacherDto)
        {
            // Pemeriksaan username supaya berbeda dengan yang lain
            if (await _userManager.Users.AnyAsync(x => x.UserName == teacherDto.Username))
            {
                return BadRequest("Username already in use");
            }

            if (teacherDto.BirthDate == DateOnly.MinValue)
            {
                return BadRequest("Date of birth required");
            }

            var user = new AppUser
            {
                UserName = teacherDto.Username,
                Role = teacherDto.Role,
            };

            var teacher = new Teacher
            {
                NameTeacher = teacherDto.NameTeacher,
                BirthDate = teacherDto.BirthDate,
                BirthPlace = teacherDto.BirthPlace,
                Address = teacherDto.Address,
                PhoneNumber = teacherDto.PhoneNumber,
                Nip = teacherDto.Nip,
                AppUserId = user.Id
            };

            var result = await _userManager.CreateAsync(user, teacherDto.Password);

            if (result.Succeeded)
            {
                // Simpan teacher ke dalam konteks database Anda
                _context.Teachers.Add(teacher);
                await _context.SaveChangesAsync();

                // Gunakan metode CreateUserObjectTeacher untuk membuat objek TeacherDto
                var teacherDtoResult = await CreateUserObjectTeacherGet(user);
                return teacherDtoResult; // Mengembalikan hasil dari Task<ActionResult<TeacherDto>>
            }
            return BadRequest(result.Errors);
        }

        [AllowAnonymous]
        [HttpPost("register/student")]
        public async Task<ActionResult<StudentGetDto>> RegisterStudent(RegisterStudentDto studentDto)
        {
            // Pemeriksaan username supaya berbeda dengan yang lain
            if (await _userManager.Users.AnyAsync(x => x.UserName == studentDto.Username))
            {
                return BadRequest("Username already in use");
            }

            if (studentDto.BirthDate == DateOnly.MinValue)
            {
                return BadRequest("Date of birth required");
            }

            // Memeriksa keunikan Nis
            if (await _context.Students.AnyAsync(s => s.Nis == studentDto.Nis))
            {
                return BadRequest("Nis already in use");
            }

            var selectedClass = await _context.ClassRooms.FirstOrDefaultAsync(c => c.UniqueNumber == studentDto.UniqueNumber);
            if (selectedClass == null)
            {
                return BadRequest("Selected UniqueName not found");
            }

            var user = new AppUser
            {
                UserName = studentDto.Username,
                Role = studentDto.Role,
            };

            var student = new Student
            {
                NameStudent = studentDto.NameStudent,
                BirthDate = studentDto.BirthDate,
                BirthPlace = studentDto.BirthPlace,
                Address = studentDto.Address,
                PhoneNumber = studentDto.PhoneNumber,
                Nis = studentDto.Nis,
                ParentName = studentDto.ParentName,
                Gender = studentDto.Gender,
                AppUserId = user.Id,
                ClassRoomId = selectedClass.Id
            };

            var result = await _userManager.CreateAsync(user, studentDto.Password);

            if (result.Succeeded)
            {
                // Simpan student ke dalam konteks database Anda
                _context.Students.Add(student);
                await _context.SaveChangesAsync();

                // Gunakan metode CreateUserObjectStudent untuk membuat objek StudentDto
                var studentDtoResult = await CreateUserObjectStudentGet(user);
                return studentDtoResult; // Mengembalikan hasil dari Task<ActionResult<StudentDto>>
            }
            return BadRequest(result.Errors);
        }

        // =========================== EDIT USER =========================== //
        [AllowAnonymous]
        [HttpPut("edit/student/{id}")]
        public async Task<IActionResult> UpdateStudent(Guid id, EditStudentDto studentEditDto)
        {
            // Mencari siswa berdasarkan ID
            var student = await _context.Students
                .Include(s => s.User)
                .Include(s => s.ClassRoom)
                .FirstOrDefaultAsync(s => s.Id == id);

            // Jika siswa tidak ditemukan, kembalikan response 404 Not Found
            if (student == null)
                return NotFound();

            // Update properti Address dan PhoneNumber
            student.Address = studentEditDto.Address;
            student.PhoneNumber = studentEditDto.PhoneNumber;

            // Jika UniqueNumber berubah, cari ClassRoom baru berdasarkan UniqueNumber
            if (student.ClassRoom?.UniqueNumber != studentEditDto.UniqueNumber)
            {
                var classRoom = await _context.ClassRooms.FirstOrDefaultAsync(c => c.UniqueNumber == studentEditDto.UniqueNumber);
                if (classRoom == null)
                {
                    return BadRequest("Invalid UniqueNumber");
                }

                // Update ClassRoom untuk siswa
                student.ClassRoom = classRoom;
            }

            // Simpan perubahan ke database
            _context.Students.Update(student);
            await _context.SaveChangesAsync();

            // Dapatkan AppUser terkait dengan siswa
            var user = await _userManager.FindByIdAsync(student.AppUserId.ToString());

            // Update password jika diperlukan
            if (!string.IsNullOrWhiteSpace(studentEditDto.Password))
            {
                // Langsung set password baru tanpa token
                var setPasswordResult = await _userManager.RemovePasswordAsync(user);
                if (!setPasswordResult.Succeeded)
                {
                    return BadRequest(setPasswordResult.Errors);
                }

                setPasswordResult = await _userManager.AddPasswordAsync(user, studentEditDto.Password);
                if (!setPasswordResult.Succeeded)
                {
                    return BadRequest(setPasswordResult.Errors);
                }
            }

            // Kembalikan response 200 OK bersama dengan data siswa yang telah diperbarui
            var updatedStudentDto = new EditStudentDto
            {
                Address = student.Address,
                PhoneNumber = student.PhoneNumber,
                UniqueNumber = student.ClassRoom?.UniqueNumber ?? string.Empty,
                Password = string.Empty // Tidak ada akses langsung ke Password dari AppUser
            };

            return Ok(updatedStudentDto);
        }

        // =========================== GET USER LOGIN =========================== //
        [Authorize]
        [HttpGet("superadmin")]
        public async Task<ActionResult<SuperAdminGetDto>> GetUserSuperAdmin()
        {
            var username = User.Identity.Name; // Mendapatkan nama pengguna dari token
            var user = await _userManager.FindByNameAsync(username);

            if (user == null)
            {
                return NotFound(); // Jika pengguna tidak ditemukan, kembalikan 404 Not Found
            }

            var superAdminDto = await CreateUserObjectSuperAdminGet(user);
            return superAdminDto;
        }

        [Authorize]
        [HttpGet("admin")]
        public async Task<ActionResult<AdminGetDto>> GetUserAdmin()
        {
            var username = User.Identity.Name; // Mendapatkan nama pengguna dari token
            var user = await _userManager.FindByNameAsync(username);

            if (user == null)
            {
                return NotFound(); // Jika pengguna tidak ditemukan, kembalikan 404 Not Found
            }

            var adminDto = await CreateUserObjectAdminGet(user);
            return adminDto;
        }

        [Authorize]
        [HttpGet("teacher")]
        public async Task<ActionResult<TeacherGetDto>> GetUserTeacher()
        {
            var username = User.Identity.Name; // Mendapatkan nama pengguna dari token
            var user = await _userManager.FindByNameAsync(username);

            if (user == null)
            {
                return NotFound(); // Jika pengguna tidak ditemukan, kembalikan 404 Not Found
            }

            var teacherDto = await CreateUserObjectTeacherGet(user);
            return teacherDto;
        }

        [Authorize]
        [HttpGet("student")]
        public async Task<ActionResult<StudentGetDto>> GetUserStudent()
        {
            var username = User.Identity.Name; // Mendapatkan nama pengguna dari token
            var user = await _userManager.FindByNameAsync(username);

            if (user == null)
            {
                return NotFound(); // Jika pengguna tidak ditemukan, kembalikan 404 Not Found
            }

            var studentDto = await CreateUserObjectStudentGet(user);
            return studentDto;
        }

        // =========================== SHORT CODE =========================== //
        private async Task<SuperAdminDto> CreateUserObjectSuperAdmin(AppUser user)
        {
            // Ambil data admin terkait dari database
            var superAdmin = await _context.SuperAdmins.FirstOrDefaultAsync(g => g.AppUserId == user.Id);

            if (superAdmin == null)
            {
                // Handle jika data superAdmin tidak ditemukan
                throw new Exception("SuperAdmin data not found");
            }

            return new SuperAdminDto
            {
                Role = user.Role,
                Username = user.UserName,
                Token = _tokenService.CreateTokenSuperAdmin(user, superAdmin),
                NameSuperAdmin = superAdmin.NameSuperAdmin,
            };
        }

        private async Task<SuperAdminGetDto> CreateUserObjectSuperAdminGet(AppUser user)
        {
            // Ambil data admin terkait dari database
            var superAdmin = await _context.SuperAdmins.FirstOrDefaultAsync(g => g.AppUserId == user.Id);

            if (superAdmin == null)
            {
                // Handle jika data superAdmin tidak ditemukan
                throw new Exception("SuperAdmin data not found");
            }

            return new SuperAdminGetDto
            {
                Role = user.Role,
                Username = user.UserName,
                NameSuperAdmin = superAdmin.NameSuperAdmin,
            };
        }

        private async Task<AdminDto> CreateUserObjectAdmin(AppUser user)
        {
            // Ambil data admin terkait dari database
            var admin = await _context.Admins.FirstOrDefaultAsync(g => g.AppUserId == user.Id);

            if (admin == null)
            {
                // Handle jika data admin tidak ditemukan
                throw new Exception("Admin data not found");
            }

            return new AdminDto
            {
                Role = user.Role,
                Username = user.UserName,
                Token = _tokenService.CreateTokenAdmin(user, admin),
                NameAdmin = admin.NameAdmin,
            };
        }

        private async Task<AdminGetDto> CreateUserObjectAdminGet(AppUser user)
        {
            // Ambil data admin terkait dari database
            var admin = await _context.Admins.FirstOrDefaultAsync(g => g.AppUserId == user.Id);

            if (admin == null)
            {
                // Handle jika data admin tidak ditemukan
                throw new Exception("Admin data not found");
            }

            return new AdminGetDto
            {
                Role = user.Role,
                Username = user.UserName,
                NameAdmin = admin.NameAdmin,
            };
        }

        private async Task<TeacherDto> CreateUserObjectTeacher(AppUser user)
        {
            // Ambil data teacher terkait dari database
            var teacher = await _context.Teachers.FirstOrDefaultAsync(g => g.AppUserId == user.Id);

            if (teacher == null)
            {
                // Handle jika data teacher tidak ditemukan
                throw new Exception("Teacher data not found");
            }

            return new TeacherDto
            {
                Role = user.Role,
                Username = user.UserName,
                Token = _tokenService.CreateTokenTeacher(user, teacher),
                NameTeacher = teacher.NameTeacher,
                BirthDate = teacher.BirthDate,
                BirthPlace = teacher.BirthPlace,
                Address = teacher.Address,
                PhoneNumber = teacher.PhoneNumber,
                Nip = teacher.Nip,
            };
        }

        private async Task<TeacherGetDto> CreateUserObjectTeacherGet(AppUser user)
        {
            // Ambil data teacher terkait dari database
            var teacher = await _context.Teachers.FirstOrDefaultAsync(g => g.AppUserId == user.Id);

            if (teacher == null)
            {
                // Handle jika data teacher tidak ditemukan
                throw new Exception("Teacher data not found");
            }

            return new TeacherGetDto
            {
                Role = user.Role,
                Username = user.UserName,
                NameTeacher = teacher.NameTeacher,
                BirthDate = teacher.BirthDate,
                BirthPlace = teacher.BirthPlace,
                Address = teacher.Address,
                PhoneNumber = teacher.PhoneNumber,
                Nip = teacher.Nip,
            };
        }

        private async Task<StudentDto> CreateUserObjectStudent(AppUser user)
        {
            // Ambil data student terkait dari database
            var student = await _context.Students
                .Include(s => s.ClassRoom)
                .FirstOrDefaultAsync(g => g.AppUserId == user.Id);

            if (student == null)
            {
                // Handle jika data student tidak ditemukan
                throw new Exception("Student data not found");
            }

            return new StudentDto
            {
                Role = user.Role,
                Username = user.UserName,
                Token = _tokenService.CreateTokenStudent(user, student),
                NameStudent = student.NameStudent,
                BirthDate = student.BirthDate,
                BirthPlace = student.BirthPlace,
                Address = student.Address,
                PhoneNumber = student.PhoneNumber,
                Nis = student.Nis,
                ParentName = student.ParentName,
                Gender = student.Gender,
                ClassName = student.ClassRoom.ClassName,
            };
        }

        private async Task<StudentGetDto> CreateUserObjectStudentGet(AppUser user)
        {
            // Ambil data student terkait dari database
            var student = await _context.Students.AsNoTracking()
                .Include(s => s.ClassRoom) // Sertakan entitas ClassRoom
                .FirstOrDefaultAsync(g => g.AppUserId == user.Id);

            if (student == null)
            {
                // Handle jika data student tidak ditemukan
                throw new Exception("Student data not found");
            }

            // Ambil nama kelas jika ada, atau beri nilai default jika tidak ada
            var className = student.ClassRoom != null ? student.ClassRoom.ClassName : "Unknown";

            return new StudentGetDto
            {
                Role = user.Role,
                Username = user.UserName,
                NameStudent = student.NameStudent,
                BirthDate = student.BirthDate,
                BirthPlace = student.BirthPlace,
                Address = student.Address,
                PhoneNumber = student.PhoneNumber,
                Nis = student.Nis,
                ParentName = student.ParentName,
                Gender = student.Gender,
                ClassName = className, // Gunakan nilai className yang telah ditentukan
            };
        }
    }
}