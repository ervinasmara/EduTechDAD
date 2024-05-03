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

namespace Application.User.Students.Command
{
    public class CreateStudentWithExcel
    {
        public class UploadStudentExcelCommand : IRequest<Result<List<RegisterStudentExcelDto>>>
        {
            public IFormFile File { get; set; }
            //public RegisterStudentExcelDto ExcelDto { get; set; }
        }

        public class RegisterStudentCommandValidator : AbstractValidator<RegisterStudentExcelDto>
        {
            public RegisterStudentCommandValidator()
            {
                RuleFor(x => x.NameStudent).NotEmpty();
                RuleFor(x => x.BirthDate).NotEmpty().WithMessage("BirthDate is required");
                RuleFor(x => x.BirthPlace).NotEmpty();
                RuleFor(x => x.Address).NotEmpty();
                RuleFor(x => x.PhoneNumber).NotEmpty().Matches("^[0-9]{8,13}$").WithMessage("Phone number must be between 8 and 13 digits and contain only numbers.");
                RuleFor(x => x.Nis).NotEmpty();
                RuleFor(x => x.ParentName).NotEmpty();
                RuleFor(x => x.Gender)
                    .NotEmpty().WithMessage("Gender is required")
                    .Must(gender => gender >= 1 && gender <= 2)
                    .WithMessage("Invalid Gender value. Use 1 for Male, 2 for Female");
                RuleFor(x => x.Username).NotEmpty();
                RuleFor(x => x.Password).NotEmpty().Matches("(?=.*\\d)(?=.*[a-z])(?=.*[A-Z]).{8,16}$").WithMessage("Password Harus Rumit");
                RuleFor(x => x.Role).NotEmpty().Equal(3).WithMessage("Role must be 3");
                RuleFor(x => x.UniqueNumberOfClassRoom).NotEmpty();
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
                var validator = new RegisterStudentCommandValidator();
                var errors = new List<string>();

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

                            var validationResult = validator.Validate(studentDto);
                            if (!validationResult.IsValid)
                            {
                                errors.AddRange(validationResult.Errors.Select(e => e.ErrorMessage));
                                continue; // Skip the current iteration if validation fails
                            }

                            // Gunakan AutoMapper untuk memetakan RegisterStudentExcelDto ke AppUser
                            var user = _mapper.Map<AppUser>(studentDto);

                            // Lakukan validasi username
                            if (await _userManager.Users.AnyAsync(x => x.UserName == user.UserName))
                            {
                                return Result<List<RegisterStudentExcelDto>>.Failure($"Username {user.UserName} already in use");
                            }

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

                            // Alur kontrol untuk validasi
                            if (!validationResult.IsValid)
                            {
                                errors.AddRange(validationResult.Errors.Select(e => e.ErrorMessage));
                                continue; // Skip the current iteration if validation fails
                            }

                            // Alur kontrol untuk pembuatan pengguna dan siswa
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

                            // Alur kontrol untuk menangani kesalahan validasi
                            if (errors.Any())
                            {
                                return Result<List<RegisterStudentExcelDto>>.Failure(string.Join(", ", errors));
                            }
                        }
                    }
                }

                return Result<List<RegisterStudentExcelDto>>.Success(students);
            }
        }
    }
}
