using Application.Core;
using MediatR;
using Persistence;
using Microsoft.EntityFrameworkCore;
using Domain.User;
using Microsoft.AspNetCore.Identity;
using Application.User.DTOs.Registration;
using FluentValidation;
using Application.User.Validation;
using System.Text.RegularExpressions;
using AutoMapper;

namespace Application.User.Students
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
                RuleFor(x => x.StudentDto).SetValidator(new RegisterStudentValidator());
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
                RegisterStudentDto studentDto = request.StudentDto;

                if (studentDto.BirthDate == DateOnly.MinValue)
                {
                    return Result<RegisterStudentDto>.Failure("Date of birth required");
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
                    return Result<RegisterStudentDto>.Failure("Selected UniqueNumberOfClass not found");
                }

                var username = newNis;

                if (await _userManager.Users.AnyAsync(x => x.UserName == username))
                {
                    return Result<RegisterStudentDto>.Failure("Username already in use");
                }

                var user = new AppUser
                {
                    UserName = username,
                    Role = 3,
                };

                await _userManager.CreateAsync(user);

                var student = _mapper.Map<Student>(studentDto);
                student.Nis = newNis;
                student.AppUserId = user.Id;
                student.ClassRoomId = selectedClass.Id;

                var password = $"{newNis}Edu#";

                if (!Regex.IsMatch(password, "(?=.*\\d)(?=.*[a-z])(?=.*[A-Z])(?!.*\\s).{8,16}"))
                {
                    return Result<RegisterStudentDto>.Failure("Password must be at least 8 characters long, contain at least one uppercase letter, one lowercase letter, and one digit.");
                }

                _context.Students.Add(student);
                await _context.SaveChangesAsync(cancellationToken);

                var studentDtoResult = _mapper.Map<RegisterStudentDto>(student);

                /** Langkah 10: Kembalikan hasil yang berhasil **/
                return Result<RegisterStudentDto>.Success(studentDtoResult);
            }
        }
    }
}