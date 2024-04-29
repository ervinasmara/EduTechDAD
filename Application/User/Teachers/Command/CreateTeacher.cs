using Application.Core;
using MediatR;
using Persistence;
using Microsoft.EntityFrameworkCore;
using Domain.User;
using Microsoft.AspNetCore.Identity;
using Application.User.DTOs.Registration;
using FluentValidation;
using Application.User.Validation;
using Domain.Many_to_Many;

namespace Application.User.Teachers.Command
{
    public class CreateTeacher
    {
        public class RegisterTeacherCommand : IRequest<Result<RegisterTeacherDto>>
        {
            public RegisterTeacherDto TeacherDto { get; set; }
        }

        public class RegisterTeacherCommandValidator : AbstractValidator<RegisterTeacherCommand>
        {
            public RegisterTeacherCommandValidator()
            {
                RuleFor(x => x.TeacherDto).SetValidator(new RegisterTeacherValidator());
            }
        }

        public class RegisterTeacherCommandHandler : IRequestHandler<RegisterTeacherCommand, Result<RegisterTeacherDto>>
        {
            private readonly UserManager<AppUser> _userManager;
            private readonly DataContext _context;

            public RegisterTeacherCommandHandler(UserManager<AppUser> userManager, DataContext context)
            {
                _userManager = userManager;
                _context = context;
            }

            public async Task<Result<RegisterTeacherDto>> Handle(RegisterTeacherCommand request, CancellationToken cancellationToken)
            {
                RegisterTeacherDto teacherDto = request.TeacherDto;

                // Pemeriksaan username supaya berbeda dengan yang lain
                if (await _userManager.Users.AnyAsync(x => x.UserName == teacherDto.Username))
                {
                    return Result<RegisterTeacherDto>.Failure("Username already in use");
                }

                // Pemeriksaan NIP supaya berbeda dengan yang lain
                if (await _context.Teachers.AnyAsync(t => t.Nip == teacherDto.Nip))
                {
                    return Result<RegisterTeacherDto>.Failure("NIP already in use");
                }

                var user = new AppUser
                {
                    UserName = teacherDto.Username,
                    Role = 2,
                };

                var teacher = new Teacher
                {
                    NameTeacher = teacherDto.NameTeacher,
                    BirthDate = teacherDto.BirthDate,
                    BirthPlace = teacherDto.BirthPlace,
                    Address = teacherDto.Address,
                    PhoneNumber = teacherDto.PhoneNumber,
                    Nip = teacherDto.Nip,
                    Gender = teacherDto.Gender,
                    AppUserId = user.Id
                };

                // Dapatkan pelajaran yang valid dari input guru
                var validLessons = await _context.Lessons
                    .Where(l => teacherDto.LessonNames.Contains(l.LessonName))
                    .ToListAsync();

                if (validLessons.Count != teacherDto.LessonNames.Count)
                {
                    var invalidLessonNames = teacherDto.LessonNames.Except(validLessons.Select(l => l.LessonName));
                    return Result<RegisterTeacherDto>.Failure($"Invalid lesson names: {string.Join(", ", invalidLessonNames)}");
                }

                foreach (var lesson in validLessons)
                {
                    var teacherLesson = new TeacherLesson
                    {
                        Teacher = teacher,
                        Lesson = lesson
                    };
                    _context.TeacherLessons.Add(teacherLesson);
                }

                var result = await _userManager.CreateAsync(user, teacherDto.Password);

                if (result.Succeeded)
                {
                    await _context.SaveChangesAsync();

                    var teacherDtoResult = await CreateUserObjectTeacherGet(user);
                    return Result<RegisterTeacherDto>.Success(teacherDtoResult);
                }

                _context.Teachers.Remove(teacher);
                return Result<RegisterTeacherDto>.Failure(string.Join(",", result.Errors));
            }

            private async Task<RegisterTeacherDto> CreateUserObjectTeacherGet(AppUser user)
            {
                var teacher = await _context.Teachers
                    .Include(t => t.TeacherLessons)
                    .ThenInclude(tl => tl.Lesson)
                    .FirstOrDefaultAsync(g => g.AppUserId == user.Id);

                if (teacher == null)
                {
                    throw new Exception("Teacher data not found");
                }

                var lessonNames = teacher.TeacherLessons.Select(tl => tl.Lesson.LessonName).ToList();

                return new RegisterTeacherDto
                {
                    Username = user.UserName,
                    NameTeacher = teacher.NameTeacher,
                    BirthDate = teacher.BirthDate,
                    BirthPlace = teacher.BirthPlace,
                    Address = teacher.Address,
                    PhoneNumber = teacher.PhoneNumber,
                    Nip = teacher.Nip,
                    Gender = teacher.Gender,
                    LessonNames = lessonNames,
                };
            }
        }
    }
}
