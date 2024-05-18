using API.Controllers;
using API.Services;
using Domain.User;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Persistence;
using Application.User.DTOs;
using Application.User.DTOs.Registration;
using Application.User.Admins;
using Application.User.DTOs.Edit;
using Application.User.Teachers.Command;
using Application.User.Teachers.Query;
using AutoMapper;
using Application.User.Students.Query;
using Application.User.Students.Command;

namespace API.Controllers.Account;
[ApiController]
[Route("api/[controller]")]
public class AccountController : BaseApiController
{
    private readonly UserManager<AppUser> _userManager;
    private readonly TokenService _tokenService;
    private readonly DataContext _context;
    private readonly IMapper _mapper;

    public AccountController(UserManager<AppUser> userManager, TokenService tokenService, DataContext context, IMapper mapper)
    {
        _userManager = userManager;
        _tokenService = tokenService;
        _context = context;
        _mapper = mapper;
    }

    // =========================== GET DATA =========================== //
    /** Get All Students **/
    [Authorize(Policy = "RequireRole1OrRole2")]
    [HttpGet("students")]
    public async Task<IActionResult> GetStudents(CancellationToken ct)
    {
        return HandleResult(await Mediator.Send(new ListStudent.Query(), ct));
    }

    /** Get Student By StudentId **/
    [Authorize(Policy = "RequireRole1Or2Or3")]
    [HttpGet("student/{id}")]
    public async Task<ActionResult> GetStudentById(Guid id, CancellationToken ct)
    {
        return HandleResult(await Mediator.Send(new DetailsStudent.Query { StudentId = id }, ct));
    }

    /** Get All Teachers **/
    [Authorize(Policy = "RequireRole1OrRole2")]
    [HttpGet("teachers")]
    public async Task<IActionResult> GetTeachers(CancellationToken ct)
    {
        return HandleResult(await Mediator.Send(new ListTeacher.Query(), ct));
    }

    /** Get Teacher By TeacherId **/
    [Authorize(Policy = "RequireRole1Or2Or3")]
    [HttpGet("teacher/{id}")]
    public async Task<ActionResult> GetTeacherByTeacherId(Guid id, CancellationToken ct)
    {
        return HandleResult(await Mediator.Send(new DetailsTeacher.Query { TeacherId = id }, ct));
    }

    /** Get Calculate Teachers and Students **/
    [Authorize(Policy = "RequireRole1OrRole2")]
    [HttpGet("calculateTeacherStudent")]
    public async Task<IActionResult> GetUsersCalculate(CancellationToken ct)
    {
        return HandleResult(await Mediator.Send(new ActiveCountUser.ActiveCountQuery(), ct));
    }

    // =========================== REGISTER =========================== //
    /** Create Admin **/
    [Authorize(Policy = "RequireRole1")]
    [HttpPost("register/admin")]
    public async Task<IActionResult> RegisterAdminCQRS(RegisterAdminDto adminDto, CancellationToken ct)
    {
        return HandleResult(await Mediator.Send(new CreateAdmin.RegisterAdminCommand { AdminDto = adminDto }, ct));
    }

    /** Create Teacher **/
    [Authorize(Policy = "RequireRole1")]
    [HttpPost("register/teacher")]
    public async Task<IActionResult> RegisterTeacherCQRS(RegisterTeacherDto teacherDto, CancellationToken ct)
    {
        return HandleResult(await Mediator.Send(new CreateTeacher.RegisterTeacherCommand { TeacherDto = teacherDto }, ct));
    }

    /** Create Student **/
    [Authorize(Policy = "RequireRole1")]
    [HttpPost("register/student2")]
    public async Task<IActionResult> RegisterStudentCQRS2(RegisterStudentDto studentDto, CancellationToken ct)
    {
        return HandleResult(await Mediator.Send(new CreateStudent.RegisterStudentCommand { StudentDto = studentDto }, ct));
    }

    /** Create StudentExcel **/
    [Authorize(Policy = "RequireRole1")]
    [HttpPost("seedexcel")]
    public async Task<IActionResult> UploadStudentExcel(IFormFile file, CancellationToken ct)
    {
        return HandleResult(await Mediator.Send(new CreateStudentWithExcel.UploadStudentExcelCommand { File = file }, ct));
    }

    [Authorize(Policy = "RequireRole1")]
    [HttpPost("seedexcel2")]
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
    /** Edit Student **/
    [Authorize(Policy = "RequireRole1")]
    [HttpPut("edit/student/{id}")]
    public async Task<IActionResult> EditStudentByStudentId(Guid id, EditStudentDto editStudentDto, CancellationToken ct)
    {
        var result = await Mediator.Send(new EditStudent.UpdateStudentCommand { StudentId = id, StudentEditDto = editStudentDto }, ct);
        return HandleResult(result);
    }

    /** Edit Teacher **/
    [Authorize(Policy = "RequireRole1")]
    [HttpPut("teacher/{id}")]
    public async Task<IActionResult> EditTeacherByTeacherId(Guid id, EditTeacherDto editTeacherDto, CancellationToken ct)
    {
        var result = await Mediator.Send(new EditTeacher.EditTeacherCommand { TeacherId = id, TeacherDto = editTeacherDto }, ct);
        return HandleResult(result);
    }

    // =========================== DEACTIVATE USER =========================== //
    [Authorize(Policy = "RequireRole4")]
    [HttpPut("admin/delete/{adminId}")]
    public async Task<IActionResult> DeleteAdmin(Guid adminId, CancellationToken ct)
    {
        var result = await Mediator.Send(new DeactivateAdmin.Command { AdminId = adminId }, ct);

        return HandleResult(result);
    }

    [Authorize(Policy = "RequireRole1")]
    [HttpPut("teacher/delete/{teacherId}")]
    public async Task<IActionResult> DeleteTeacher(Guid teacherId, CancellationToken ct)
    {
        var result = await Mediator.Send(new DeactivateTeacher.Command { TeacherId = teacherId }, ct);

        return HandleResult(result);
    }

    [Authorize(Policy = "RequireRole1")]
    [HttpPut("student/delete/{studentId}")]
    public async Task<IActionResult> DeleteStudent(Guid studentId, CancellationToken ct)
    {
        var result = await Mediator.Send(new DeactivateStudent.Command { StudentId = studentId }, ct);

        return HandleResult(result);
    }

    // =========================== LOGIN =========================== //
    /** LOGIN USER **/
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

    // =========================== GET USER LOGIN =========================== //
    /** Get Info Who User Login **/
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
            default:
                return BadRequest("Role not valid"); // Kembalikan 400 Bad Request jika peran tidak valid
        }

        return userDto;
    }

    // =========================== SHORT CODE =========================== //
    /** LOGIN ADMIN **/
    private async Task<AdminDto> CreateUserObjectAdmin(AppUser user)
    {
        var admin = await _context.Admins.FirstOrDefaultAsync(g => g.AppUserId == user.Id);

        if (admin == null)
        {
            throw new Exception("Admin data not found");
        }

        // Pemetaan menggunakan AutoMapper
        var adminDto = _mapper.Map<AdminDto>(user);
        _mapper.Map(admin, adminDto); // Pemetaan tambahan untuk properti dari Admin

        // Atur token secara manual
        adminDto.Token = _tokenService.CreateTokenAdmin(user, admin);

        return adminDto;
    }

    /** USERINFO ADMIN **/
    private async Task<AdminGetDto> CreateUserObjectAdminGet(AppUser user)
    {
        // Ambil data admin terkait dari database
        var admin = await _context.Admins.FirstOrDefaultAsync(g => g.AppUserId == user.Id);

        if (admin == null)
        {
            // Handle jika data admin tidak ditemukan
            throw new Exception("Admin data not found");
        }

        // Gunakan mapper untuk memetakan AppUser dan Admin ke AdminGetDto
        var adminGetDto = _mapper.Map<AdminGetDto>(user);
        _mapper.Map(admin, adminGetDto);

        return adminGetDto;
    }

    /** LOGIN TEACHER **/
    private async Task<TeacherDto> CreateUserObjectTeacher(AppUser user)
    {
        // Ambil data teacher terkait dari database
        var teacher = await _context.Teachers.FirstOrDefaultAsync(g => g.AppUserId == user.Id);

        if (teacher == null)
        {
            // Handle jika data teacher tidak ditemukan
            throw new Exception("Teacher data not found");
        }

        // Pemetaan menggunakan AutoMapper
        var teacherDto = _mapper.Map<TeacherDto>(user);
        _mapper.Map(teacher, teacherDto); // Pemetaan tambahan untuk properti dari Teacher

        // Atur token secara manual
        teacherDto.Token = _tokenService.CreateTokenTeacher(user, teacher);

        return teacherDto;
    }

    /** USERINFO TEACHER **/
    private async Task<TeacherRegisterDto> CreateUserObjectTeacherGet(AppUser user)
    {
        // Ambil data teacher terkait dari database
        var teacher = await _context.Teachers.FirstOrDefaultAsync(g => g.AppUserId == user.Id);

        if (teacher == null)
        {
            // Handle jika data teacher tidak ditemukan
            throw new Exception("Teacher data not found");
        }

        // Gunakan mapper untuk memetakan AppUser dan Teacher ke TeacherGetDto
        var teacherGetDto = _mapper.Map<TeacherRegisterDto>(user);
        _mapper.Map(teacher, teacherGetDto);

        return teacherGetDto;
    }

    /** LOGIN STUDENT **/
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

        // Pemetaan menggunakan AutoMapper
        var studentDto = _mapper.Map<StudentDto>(user);
        _mapper.Map(student, studentDto); // Pemetaan tambahan untuk properti dari Student

        // Atur token secara manual
        studentDto.Token = _tokenService.CreateTokenStudent(user, student);

        return studentDto;
    }

    /** USERINFO STUDENT **/
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

        // Gunakan mapper untuk memetakan AppUser dan Student ke StudentGetDto
        var studentGetDto = _mapper.Map<StudentGetDto>(user);
        _mapper.Map(student, studentGetDto);

        return studentGetDto;
    }
}