using API.Controllers;
using API.Services;
using Domain.User;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Persistence;
using Application.User.Students;
using Application.User.DTOs;
using Application.User.DTOs.Registration;
using Application.User.Superadmin;
using Application.User.Admins;
using Application.User.Teachers;
using Application.User.DTOs.Edit;

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
        [Authorize(Policy = "RequireRole1,2,3,4")]
        [HttpGet("students")]
        public async Task<IActionResult> GetStudents(CancellationToken ct)
        {
            return HandleResult(await Mediator.Send(new ListStudent.Query(), ct));
        }

        [Authorize(Policy = "RequireRole1,2,3,4")]
        [HttpGet("student/{id}")]
        public async Task<ActionResult> GetStudentById(Guid id, CancellationToken ct)
        {
            return HandleResult(await Mediator.Send(new DetailsStudentById.Query { Id = id }, ct));
        }

        [Authorize(Policy = "RequireRole1,2,4")]
        [HttpGet("studentparam")]
        public async Task<ActionResult> GetStudentParam([FromQuery] string nis, [FromQuery] string name, [FromQuery] string className, CancellationToken ct)
        {
            return HandleResult(await Mediator.Send(new DetailsStudent.Query { Nis = nis, Name = name, ClassName = className }, ct));
        }

        [Authorize(Policy = "RequireRole1,2,4")]
        [HttpGet("teachers")]
        public async Task<IActionResult> GetTeachers(CancellationToken ct)
        {
            return HandleResult(await Mediator.Send(new ListTeacher.Query(), ct));
        }

        [Authorize(Policy = "RequireRole1,2,3,4")]
        [HttpGet("teacher/{id}")]
        public async Task<ActionResult> GetTeacherByTeacherId(Guid id, CancellationToken ct)
        {
            return HandleResult(await Mediator.Send(new DetailsTeacher.Query { Id = id }, ct));
        }

        [Authorize(Policy = "RequireRole1OrRole4")]
        [HttpPut("teacher/{teacherId}")]
        public async Task<IActionResult> EditTeacherByTeacherId(Guid teacherId, EditTeacherDto editTeacherDto, CancellationToken ct)
        {
            var result = await Mediator.Send(new EditTeacher.EditTeacherCommand { TeacherId = teacherId, TeacherDto = editTeacherDto }, ct);

            return HandleResult(result);
        }

        [Authorize(Policy = "RequireRole4")]
        [HttpPut("admin/delete/{adminId}")]
        public async Task<IActionResult> DeleteAdmin(Guid adminId, CancellationToken ct)
        {
            var result = await Mediator.Send(new DeactivateAdmin.Command { AdminId = adminId}, ct);

            return HandleResult(result);
        }

        // =========================== LOGIN =========================== //
        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<ActionResult<object>> Login(LoginDto loginDto)
        {
            var user = await _userManager.FindByNameAsync(loginDto.Username);

            if (user == null)
                return Unauthorized();

            var result = await _userManager.CheckPasswordAsync(user, loginDto.Password);

            if (result)
            {
                try
                {
                    switch (user.Role)
                    {
                        case 1: // Admin
                            var admin = await _context.Admins.FirstOrDefaultAsync(a => a.AppUserId == user.Id);
                            if (admin != null && admin.Status == 0)
                                return Unauthorized();
                            return await CreateUserObjectAdmin(user);
                        case 2: // Teacher
                            var teacher = await _context.Teachers.FirstOrDefaultAsync(t => t.AppUserId == user.Id);
                            if (teacher != null && teacher.Status == 0)
                                return Unauthorized();
                            return await CreateUserObjectTeacher(user);
                        case 3: // Student
                            var student = await _context.Students.FirstOrDefaultAsync(s => s.AppUserId == user.Id);
                            if (student != null && student.Status == 0)
                                return Unauthorized();
                            return await CreateUserObjectStudent(user);
                        case 4: // Super Admin
                            var superAdmin = await _context.SuperAdmins.FirstOrDefaultAsync(sa => sa.AppUserId == user.Id);
                            if (superAdmin != null && superAdmin.Status == 0)
                                return Unauthorized();
                            return await CreateUserObjectSuperAdmin(user);
                        default:
                            return BadRequest("Invalid role");
                    }
                }
                catch (Exception ex)
                {
                    return BadRequest(ex.Message);
                }
            }
            return Unauthorized();
        }

        // =========================== REGISTER =========================== //
        [Authorize(Policy = "RequireRole4")]
        [HttpPost("register/superAdmin")]
        public async Task<IActionResult> RegisterSuperAdminCQRS(RegisterSuperAdminDto superAdminDto, CancellationToken ct)
        {
            return HandleResult(await Mediator.Send(new CreateSuperAdmin.RegisterSuperAdminCommand { SuperAdminDto = superAdminDto }, ct));
        }

        [Authorize(Policy = "RequireRole1OrRole4")]
        [HttpPost("register/admin")]
        public async Task<IActionResult> RegisterAdminCQRS(RegisterAdminDto adminDto, CancellationToken ct)
        {
            return HandleResult(await Mediator.Send(new CreateAdmin.RegisterAdminCommand { AdminDto = adminDto }, ct));
        }

        [Authorize(Policy = "RequireRole1OrRole4")]
        [HttpPost("register/teacher")]
        public async Task<IActionResult> RegisterTeacherCQRS(RegisterTeacherDto teacherDto, CancellationToken ct)
        {
            return HandleResult(await Mediator.Send(new CreateTeacher.RegisterTeacherCommand { TeacherDto = teacherDto }, ct));
        }

        [Authorize(Policy = "RequireRole1OrRole4")]
        [HttpPost("register/student")]
        public async Task<IActionResult> RegisterStudentCQRS(RegisterStudentDto studentDto, CancellationToken ct)
        {
            return HandleResult(await Mediator.Send(new CreateStudent.RegisterStudentCommand { StudentDto = studentDto }, ct));
        }

        [Authorize(Policy = "RequireRole1OrRole4")]
        [HttpPost("seedexcel")]
        public async Task<IActionResult> UploadExcel4(IFormFile file)
        {
            if (file == null || file.Length <= 0)
            {
                return BadRequest("Invalid file");
            }

            try
            {
                using (var stream = new MemoryStream())
                {
                    await file.CopyToAsync(stream);
                    stream.Position = 0;

                    // Baca data dari file Excel dan simpan ke database
                    using (var package = new OfficeOpenXml.ExcelPackage(stream))
                    {
                        var worksheet = package.Workbook.Worksheets[0]; // Misalnya, data berada di worksheet pertama
                        var rowCount = worksheet.Dimension.Rows;

                        for (int row = 2; row <= rowCount; row++) // Mulai dari baris kedua, karena baris pertama mungkin berisi header
                        {
                            var studentDto = new RegisterStudentExcelDto
                            {
                                NameStudent = worksheet.Cells[row, 1].Value.ToString(),
                                BirthDate = DateOnly.Parse(worksheet.Cells[row, 2].Value.ToString()),
                                BirthPlace = worksheet.Cells[row, 3].Value.ToString(),
                                Address = worksheet.Cells[row, 4].Value.ToString(),
                                PhoneNumber = worksheet.Cells[row, 5].Value.ToString(),
                                Nis = worksheet.Cells[row, 6].Value.ToString(),
                                ParentName = worksheet.Cells[row, 7].Value.ToString(),
                                Gender = Convert.ToInt32(worksheet.Cells[row, 8].Value.ToString()),
                                UniqueNumberOfClassRoom = worksheet.Cells[row, 9].Value.ToString(),
                                Username = worksheet.Cells[row, 6].Value.ToString(), // Menggunakan NIS sebagai username
                                Password = worksheet.Cells[row, 10].Value.ToString(), // Tentukan kata sandi default atau sesuaikan dengan kebutuhan Anda
                                Role = 3 // Tentukan peran default atau sesuaikan dengan kebutuhan Anda
                            };


                            // Simpan data student dari file Excel
                            var result = await RegisterStudentExcel(studentDto);

                            if (result is BadRequestObjectResult badRequest)
                            {
                                return badRequest;
                            }
                        }
                    }
                }

                return Ok("Data from Excel uploaded and saved successfully");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred while uploading Excel file: {ex.Message}");
            }
        }
        private async Task<IActionResult> RegisterStudentExcel(RegisterStudentExcelDto studentDto)
        {
            // Pemeriksaan username supaya berbeda dengan yang lain
            if (await _userManager.Users.AnyAsync(x => x.UserName == studentDto.Username))
            {
                return BadRequest($"Username {studentDto.Username} already in use");
            }

            if (studentDto.BirthDate == DateOnly.MinValue)
            {
                return BadRequest("Date of birth required");
            }

            if (string.IsNullOrEmpty(studentDto.Username))
            {
                return BadRequest("Username is required");
            }

            if (string.IsNullOrEmpty(studentDto.Password))
            {
                return BadRequest("Password is required");
            }

            // Memeriksa keunikan Nis
            if (await _context.Students.AnyAsync(s => s.Nis == studentDto.Nis))
            {
                return BadRequest($"Nis {studentDto.Nis} already in use");
            }

            var selectedClass = await _context.ClassRooms.FirstOrDefaultAsync(c => c.UniqueNumberOfClassRoom == studentDto.UniqueNumberOfClassRoom);
            if (selectedClass == null)
            {
                return BadRequest("Selected UniqueNumberOfClass not found");
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

                return Ok("Student registered successfully");
            }

            return BadRequest(result.Errors);
        }

        // =========================== EDIT USER =========================== //
        [Authorize(Policy = "RequireRole1OrRole4")]
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

            // Jika UniqueNumberOfClassRoom berubah, cari ClassRoom baru berdasarkan UniqueNumberOfClassRoom
            if (student.ClassRoom?.UniqueNumberOfClassRoom != studentEditDto.UniqueNumberOfClassRoom)
            {
                var classRoom = await _context.ClassRooms.FirstOrDefaultAsync(c => c.UniqueNumberOfClassRoom == studentEditDto.UniqueNumberOfClassRoom);
                if (classRoom == null)
                {
                    return BadRequest("Invalid UniqueNumberOfClassRoom");
                }

                // Update ClassRoom untuk siswa
                student.ClassRoom = classRoom;
            }

            // Simpan perubahan ke database
            _context.Students.Update(student);
            await _context.SaveChangesAsync();

            // Dapatkan AppUser terkait dengan siswa
            var user = await _userManager.FindByIdAsync(student.AppUserId.ToString());

            // Kembalikan response 200 OK bersama dengan data siswa yang telah diperbarui
            var updatedStudentDto = new EditStudentDto
            {
                Address = student.Address,
                PhoneNumber = student.PhoneNumber,
                UniqueNumberOfClassRoom = student.ClassRoom?.UniqueNumberOfClassRoom ?? string.Empty,
            };

            return Ok(updatedStudentDto);
        }

        // =========================== GET USER LOGIN =========================== //
        [Authorize]
        [HttpGet("userinfo")]
        public async Task<ActionResult<object>> GetUserInfo()
        {
            var username = User.Identity.Name; // Mendapatkan nama pengguna dari token
            var user = await _userManager.FindByNameAsync(username);

            if (user == null)
            {
                return NotFound(); // Jika pengguna tidak ditemukan, kembalikan 404 Not Found
            }

            object userDto;

            switch (user.Role)
            {
                case 1:
                    userDto = await CreateUserObjectAdminGet(user);
                    break;
                case 2:
                    userDto = await CreateUserObjectTeacherGet(user);
                    break;
                case 3:
                    userDto = await CreateUserObjectStudentGet(user);
                    break;
                case 4:
                    userDto = await CreateUserObjectSuperAdminGet(user);
                    break;
                default:
                    return BadRequest("Role not valid"); // Kembalikan 400 Bad Request jika peran tidak valid
            }

            return userDto;
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

        private async Task<TeacherRegisterDto> CreateUserObjectTeacherGet(AppUser user)
        {
            // Ambil data teacher terkait dari database
            var teacher = await _context.Teachers.FirstOrDefaultAsync(g => g.AppUserId == user.Id);

            if (teacher == null)
            {
                // Handle jika data teacher tidak ditemukan
                throw new Exception("Teacher data not found");
            }

            return new TeacherRegisterDto
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