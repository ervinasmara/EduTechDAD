using Application.Core;
using MediatR;
using Persistence;
using Microsoft.EntityFrameworkCore;
using Domain.User;
using Microsoft.AspNetCore.Identity;
using Application.User.DTOs.Registration;
using FluentValidation;
using System.Text.RegularExpressions;
using AutoMapper;

namespace Application.User.Students.Command
{
    public class CreateStudent
    {
        public class RegisterStudentCommand : IRequest<Result<RegisterStudentDto>>
        {
            public RegisterStudentDto StudentDto { get; set; }
        }

        public class RegisterStudentCommandValidator : AbstractValidator<RegisterStudentCommand>
        {
            public RegisterStudentCommandValidator()
            {
                RuleFor(x => x.StudentDto.NameStudent).NotEmpty().WithMessage("Name is required");
                RuleFor(x => x.StudentDto.BirthDate).NotEmpty().WithMessage("BirthDate is required");
                RuleFor(x => x.StudentDto.BirthPlace).NotEmpty().WithMessage("BirthPlace is required");
                RuleFor(x => x.StudentDto.Address).NotEmpty().WithMessage("Address is required");
                RuleFor(x => x.StudentDto.PhoneNumber)
                    .NotEmpty().WithMessage("Phone number is required")
                    .Matches("^[0-9]{8,13}$").WithMessage("Phone number must be between 8 and 13 digits and contain only numbers.");
                RuleFor(x => x.StudentDto.ParentName).NotEmpty().WithMessage("ParentName is required");
                RuleFor(x => x.StudentDto.Gender)
                    .NotEmpty().WithMessage("Gender is required")
                    .Must(gender => gender >= 1 && gender <= 2)
                    .WithMessage("Invalid Gender value. Use 1 for Male, 2 for Female");
                RuleFor(x => x.StudentDto.UniqueNumberOfClassRoom).NotEmpty().WithMessage("UniqueNumberOfClassRoom is required");
            }
        }

        public class RegisterStudentCommandHandler : IRequestHandler<RegisterStudentCommand, Result<RegisterStudentDto>>
        {
            private readonly UserManager<AppUser> _userManager;
            private readonly DataContext _context;
            private readonly IMapper _mapper;

            public RegisterStudentCommandHandler(UserManager<AppUser> userManager, DataContext context, IMapper mapper)
            {
                _userManager = userManager;
                _context = context;
                _mapper = mapper;
            }

            public async Task<Result<RegisterStudentDto>> Handle(RegisterStudentCommand request, CancellationToken cancellationToken)
            {
                /** Langkah 1: Mendapatkan data siswa dari permintaan **/
                RegisterStudentDto studentDto = request.StudentDto;

                /** Langkah 2: Memeriksa apakah tanggal lahir telah disediakan **/
                if (studentDto.BirthDate == DateOnly.MinValue)
                {
                    return Result<RegisterStudentDto>.Failure("Date of birth required");
                }

                /** Langkah 3: Mencari nomor induk siswa (NIS) terakhir dan menyiapkan nomor NIS baru **/
                var lastNis = await _context.Students.MaxAsync(s => s.Nis);
                int newNisNumber = 1;
                if (!string.IsNullOrEmpty(lastNis))
                {
                    newNisNumber = int.Parse(lastNis) + 1;
                }
                var newNis = newNisNumber.ToString("00000");

                /** Langkah 4: Memeriksa keberadaan kelas dengan UniqueNumberOfClassRoom yang dipilih **/
                var selectedClass = await _context.ClassRooms.FirstOrDefaultAsync(c => c.UniqueNumberOfClassRoom == studentDto.UniqueNumberOfClassRoom);
                if (selectedClass == null)
                {
                    return Result<RegisterStudentDto>.Failure("Selected UniqueNumberOfClass not found");
                }

                /** Langkah 5: Menyiapkan nama pengguna untuk siswa **/
                var username = newNis;

                /** Langkah 6: Memeriksa apakah nama pengguna sudah digunakan **/
                if (await _userManager.Users.AnyAsync(x => x.UserName == username))
                {
                    return Result<RegisterStudentDto>.Failure("Username already in use");
                }

                /** Langkah 7: Membuat entitas pengguna baru **/
                var user = new AppUser
                {
                    UserName = username,
                    Role = 3, // Peran 3 mewakili peran siswa
                };

                await _userManager.CreateAsync(user);

                /** Langkah 8: Memetakan data siswa dari DTO ke entitas siswa **/
                var student = _mapper.Map<Student>(studentDto);
                student.Nis = newNis;
                student.AppUserId = user.Id;
                student.ClassRoomId = selectedClass.Id;

                /** Langkah 9: Memeriksa dan menyiapkan kata sandi siswa **/
                var password = $"{newNis}Edu#";
                if (!Regex.IsMatch(password, "(?=.*\\d)(?=.*[a-z])(?=.*[A-Z])(?!.*\\s).{8,16}"))
                {
                    return Result<RegisterStudentDto>.Failure("Password must be at least 8 characters long, contain at least one uppercase letter, one lowercase letter, and one digit.");
                }

                /** Langkah 10: Menambahkan entitas siswa baru ke konteks dan menyimpan perubahan **/
                _context.Students.Add(student);
                await _context.SaveChangesAsync(cancellationToken);

                /** Langkah 11: Memetakan entitas siswa kembali ke DTO dan mengembalikannya **/
                var studentDtoResult = _mapper.Map<RegisterStudentDto>(student);
                return Result<RegisterStudentDto>.Success(studentDtoResult);
            }
        }
    }
}