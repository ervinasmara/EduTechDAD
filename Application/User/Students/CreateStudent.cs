using Application.Core;
using MediatR;
using Persistence;
using Microsoft.EntityFrameworkCore;
using Application.User.DTOs;
using Domain.User;
using Microsoft.AspNetCore.Identity;
using Application.User.DTOs.Registration;
using FluentValidation;
using Application.User.Validation;
using System.Text.RegularExpressions;

namespace Application.User.Students
{
    public class CreateStudent
    {
        public class RegisterStudentCommand : IRequest<Result<StudentGetDto>>
        {
            public RegisterStudentDto StudentDto { get; set; }
        }

        public class RegisterStudentCommandValidator : AbstractValidator<RegisterStudentCommand>
        {
            public RegisterStudentCommandValidator()
            {
                RuleFor(x => x.StudentDto).SetValidator(new RegisterStudentValidator());
            }
        }

        public class RegisterStudentCommandHandler : IRequestHandler<RegisterStudentCommand, Result<StudentGetDto>>
        {
            private readonly UserManager<AppUser> _userManager;
            private readonly DataContext _context;

            public RegisterStudentCommandHandler(UserManager<AppUser> userManager, DataContext context)
            {
                _userManager = userManager;
                _context = context;
            }

            public async Task<Result<StudentGetDto>> Handle(RegisterStudentCommand request, CancellationToken cancellationToken)
            {
                RegisterStudentDto studentDto = request.StudentDto;

                if (studentDto.BirthDate == DateOnly.MinValue)
                {
                    return Result<StudentGetDto>.Failure("Date of birth required");
                }

                var lastNis = await _context.Students.MaxAsync(s => s.Nis);
                int newNisNumber = 1;
                if (!string.IsNullOrEmpty(lastNis))
                {
                    newNisNumber = int.Parse(lastNis) + 1;
                }

                var newNis = newNisNumber.ToString("00000");

                var selectedClass = await _context.ClassRooms.FirstOrDefaultAsync(c => c.UniqueNumberOfClassRoom == studentDto.UniqueNumberOfClassRoom);
                if (selectedClass == null)
                {
                    return Result<StudentGetDto>.Failure("Selected UniqueNumberOfClass not found");
                }

                var username = newNis;

                if (await _userManager.Users.AnyAsync(x => x.UserName == username))
                {
                    return Result<StudentGetDto>.Failure("Username already in use");
                }

                var user = new AppUser
                {
                    UserName = username,
                    Role = studentDto.Role,
                };

                var student = new Student
                {
                    NameStudent = studentDto.NameStudent,
                    BirthDate = studentDto.BirthDate,
                    BirthPlace = studentDto.BirthPlace,
                    Address = studentDto.Address,
                    PhoneNumber = studentDto.PhoneNumber,
                    Nis = newNis,
                    ParentName = studentDto.ParentName,
                    Gender = studentDto.Gender,
                    AppUserId = user.Id,
                    ClassRoomId = selectedClass.Id
                };

                var password = $"{newNis}Edu#";

                if (!Regex.IsMatch(password, "(?=.*\\d)(?=.*[a-z])(?=.*[A-Z])(?!.*\\s).{8,16}"))
                {
                    return Result<StudentGetDto>.Failure("Password must be at least 8 characters long, contain at least one uppercase letter, one lowercase letter, and one digit.");
                }

                var result = await _userManager.CreateAsync(user, password);

                if (result.Succeeded)
                {
                    _context.Students.Add(student);
                    await _context.SaveChangesAsync();

                    var studentDtoResult = await CreateUserObjectStudentGet(user);
                    return Result<StudentGetDto>.Success(studentDtoResult);
                }

                return Result<StudentGetDto>.Failure(string.Join(",", result.Errors));
            }

            private async Task<StudentGetDto> CreateUserObjectStudentGet(AppUser user)
            {
                var student = await _context.Students.AsNoTracking()
                    .Include(s => s.ClassRoom)
                    .FirstOrDefaultAsync(g => g.AppUserId == user.Id);

                if (student == null)
                {
                    throw new Exception("Student data not found");
                }

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
                    ClassName = className,
                };
            }
        }
    }
}