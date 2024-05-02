using Application.Core;
using MediatR;
using Persistence;
using Domain.User;
using Microsoft.AspNetCore.Identity;
using Application.User.DTOs.Registration;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using FluentValidation;
using System.Text.RegularExpressions;

namespace Application.User.Students
{
    public class CreateStudentWithExcel
    {
        public class UploadStudentExcelCommand : IRequest<Result<List<RegisterStudentExcelDto>>>
        {
            public IFormFile File { get; set; }
            public RegisterStudentExcelDto ExcelDto { get; set; }
        }

        public class RegisterStudentCommandValidator : AbstractValidator<UploadStudentExcelCommand>
        {
            public RegisterStudentCommandValidator()
            {
                RuleFor(x => x.ExcelDto.NameStudent).NotEmpty();
                RuleFor(x => x.ExcelDto.BirthDate).NotEmpty().WithMessage("BirthDate is required");
                RuleFor(x => x.ExcelDto.BirthPlace).NotEmpty();
                RuleFor(x => x.ExcelDto.Address).NotEmpty();
                RuleFor(x => x.ExcelDto.PhoneNumber).NotEmpty().Matches("^[0-9]{8,13}$").WithMessage("Phone number must be between 8 and 13 digits and contain only numbers.");
                RuleFor(x => x.ExcelDto.Nis).NotEmpty();
                RuleFor(x => x.ExcelDto.ParentName).NotEmpty();
                RuleFor(x => x.ExcelDto.Gender)
                    .NotEmpty().WithMessage("Gender is required")
                    .Must(gender => gender >= 1 && gender <= 2)
                    .WithMessage("Invalid Gender value. Use 1 for Male, 2 for Female");
                RuleFor(x => x.ExcelDto.Username).NotEmpty();
                RuleFor(x => x.ExcelDto.Password).NotEmpty().Matches("(?=.*\\d)(?=.*[a-z])(?=.*[A-Z]).{8,16}$").WithMessage("Password Harus Rumit");
                RuleFor(x => x.ExcelDto.Role).NotEmpty().Equal(3).WithMessage("Role must be 3");
                RuleFor(x => x.ExcelDto.UniqueNumberOfClassRoom).NotEmpty();
            }
        }

        // Command Handler
        public class UploadStudentExcelCommandHandler : IRequestHandler<UploadStudentExcelCommand, Result<List<RegisterStudentExcelDto>>>
        {
            private readonly UserManager<AppUser> _userManager;
            private readonly DataContext _context;
            private readonly IMapper _mapper;

            public UploadStudentExcelCommandHandler(UserManager<AppUser> userManager, DataContext context, IMapper mapper)
            {
                _userManager = userManager;
                _context = context;
                _mapper = mapper;
            }

            public async Task<Result<List<RegisterStudentExcelDto>>> Handle(UploadStudentExcelCommand request, CancellationToken cancellationToken)
            {
                if (request.File == null || request.File.Length <= 0)
                {
                    return Result<List<RegisterStudentExcelDto>>.Failure("Invalid file");
                }

                var students = new List<RegisterStudentExcelDto>();

                using (var stream = new MemoryStream())
                {
                    await request.File.CopyToAsync(stream);
                    stream.Position = 0;

                    using (var package = new OfficeOpenXml.ExcelPackage(stream))
                    {
                        var worksheet = package.Workbook.Worksheets[0];
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

                            // Validasi Manual
                            if (string.IsNullOrEmpty(studentDto.NameStudent))
                            {
                                return Result<List<RegisterStudentExcelDto>>.Failure("NameStudent is required");
                            }
                            //if (studentDto.BirthDate == DateOnly.MinValue)
                            //{
                            //    return Result<List<RegisterStudentExcelDto>>.Failure("BirthDate is required");
                            //}
                            if (string.IsNullOrEmpty(studentDto.BirthPlace))
                            {
                                return Result<List<RegisterStudentExcelDto>>.Failure("BirthPlace is required");
                            }
                            if (string.IsNullOrEmpty(studentDto.Address))
                            {
                                return Result<List<RegisterStudentExcelDto>>.Failure("Address is required");
                            }
                            if (string.IsNullOrEmpty(studentDto.PhoneNumber) || !Regex.IsMatch(studentDto.PhoneNumber, "^[0-9]{8,13}$"))
                            {
                                return Result<List<RegisterStudentExcelDto>>.Failure("Phone number must be between 8 and 13 digits and contain only numbers.");
                            }
                            if (string.IsNullOrEmpty(studentDto.Nis))
                            {
                                return Result<List<RegisterStudentExcelDto>>.Failure("Nis is required");
                            }
                            if (string.IsNullOrEmpty(studentDto.ParentName))
                            {
                                return Result<List<RegisterStudentExcelDto>>.Failure("ParentName is required");
                            }
                            if (studentDto.Gender != 1 && studentDto.Gender != 2)
                            {
                                return Result<List<RegisterStudentExcelDto>>.Failure("Invalid Gender value. Use 1 for Male, 2 for Female");
                            }
                            if (string.IsNullOrEmpty(studentDto.Username))
                            {
                                return Result<List<RegisterStudentExcelDto>>.Failure("Username is required");
                            }
                            if (!Regex.IsMatch(studentDto.Password, "(?=.*\\d)(?=.*[a-z])(?=.*[A-Z]).{8,16}$"))
                            {
                                return Result<List<RegisterStudentExcelDto>>.Failure("Password must be 8-16 characters long and contain at least one digit, one lowercase letter, and one uppercase letter");
                            }
                            if (studentDto.Role != 3)
                            {
                                return Result<List<RegisterStudentExcelDto>>.Failure("Role must be 3");
                            }
                            if (string.IsNullOrEmpty(studentDto.UniqueNumberOfClassRoom))
                            {
                                return Result<List<RegisterStudentExcelDto>>.Failure("UniqueNumberOfClassRoom is required");
                            }

                            // Gunakan AutoMapper untuk memetakan RegisterStudentExcelDto ke AppUser
                            var user = _mapper.Map<AppUser>(studentDto);

                            // Lakukan validasi username
                            if (await _userManager.Users.AnyAsync(x => x.UserName == user.UserName))
                            {
                                return Result<List<RegisterStudentExcelDto>>.Failure($"Username {user.UserName} already in use");
                            }

                            // Validasi lainnya ...

                            // Gunakan AutoMapper untuk memetakan RegisterStudentExcelDto ke Student
                            var student = _mapper.Map<Student>(studentDto);

                            // Lakukan validasi Nis
                            if (await _context.Students.AnyAsync(s => s.Nis == student.Nis))
                            {
                                return Result<List<RegisterStudentExcelDto>>.Failure($"Nis {student.Nis} already in use");
                            }

                            // Cari kelas unik
                            var selectedClass = await _context.ClassRooms.FirstOrDefaultAsync(c => c.UniqueNumberOfClassRoom == studentDto.UniqueNumberOfClassRoom);
                            if (selectedClass == null)
                            {
                                return Result<List<RegisterStudentExcelDto>>.Failure("Selected UniqueNumberOfClass not found");
                            }

                            // Tambahkan ID pengguna dan ID kelas ke siswa
                            student.AppUserId = user.Id;
                            student.ClassRoomId = selectedClass.Id;

                            students.Add(studentDto);
                            // Buat pengguna dan siswa
                            var createUserResult = await _userManager.CreateAsync(user, studentDto.Password);

                            if (createUserResult.Succeeded)
                            {
                                // Simpan siswa ke dalam konteks database Anda
                                _context.Students.Add(student);
                                await _context.SaveChangesAsync();
                            }
                            else
                            {
                                // Jika pembuatan pengguna gagal, kembalikan pesan kesalahan
                                return Result<List<RegisterStudentExcelDto>>.Failure(string.Join(", ", createUserResult.Errors.Select(e => e.Description)));
                            }
                        }
                    }
                }

                return Result<List<RegisterStudentExcelDto>>.Success(students);
            }
        }
    }
}
