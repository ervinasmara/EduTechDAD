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
using Domain.Many_to_Many;

namespace Application.User.Teachers
{
    public class CreateTeacher
    {
        public class RegisterTeacherCommand : IRequest<Result<TeacherRegisterDto>>
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

        public class RegisterTeacherCommandHandler : IRequestHandler<RegisterTeacherCommand, Result<TeacherRegisterDto>>
        {
            private readonly UserManager<AppUser> _userManager;
            private readonly DataContext _context;

            public RegisterTeacherCommandHandler(UserManager<AppUser> userManager, DataContext context)
            {
                _userManager = userManager;
                _context = context;
            }

            public async Task<Result<TeacherRegisterDto>> Handle(RegisterTeacherCommand request, CancellationToken cancellationToken)
            {
                RegisterTeacherDto teacherDto = request.TeacherDto;

                // Pemeriksaan username supaya berbeda dengan yang lain
                if (await _userManager.Users.AnyAsync(x => x.UserName == teacherDto.Username))
                {
                    return Result<TeacherRegisterDto>.Failure("Username already in use");
                }

                if (teacherDto.BirthDate == DateOnly.MinValue)
                {
                    return Result<TeacherRegisterDto>.Failure("Date of birth required");
                }

                // Pemeriksaan NIP supaya berbeda dengan yang lain
                if (await _context.Teachers.AnyAsync(t => t.Nip == teacherDto.Nip))
                {
                    return Result<TeacherRegisterDto>.Failure("NIP already in use");
                }

                // Pemeriksaan apakah lessonname ada di database
                foreach (var lessonName in teacherDto.LessonName)
                {
                    if (!await _context.Lessons.AnyAsync(l => l.LessonName == lessonName))
                    {
                        return Result<TeacherRegisterDto>.Failure($"Lesson '{lessonName}' does not exist");
                    }
                }

                // Pemeriksaan apakah uniquenumberofclassroom ada di database
                foreach (var uniqueNumber in teacherDto.UniqueNumberOfClassRoom)
                {
                    if (!await _context.ClassRooms.AnyAsync(c => c.UniqueNumberOfClassRoom == uniqueNumber))
                    {
                        return Result<TeacherRegisterDto>.Failure($"Classroom with unique number '{uniqueNumber}' does not exist");
                    }
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
                    // Simpan teacher ke dalam konteks database
                    _context.Teachers.Add(teacher);

                    // Tambahkan relasi antara guru dan pelajaran (Lesson)
                    foreach (var lessonName in teacherDto.LessonName)
                    {
                        var lesson = await _context.Lessons.FirstOrDefaultAsync(l => l.LessonName == lessonName);
                        if (lesson != null)
                        {
                            var teacherLesson = new TeacherLesson
                            {
                                Teacher = teacher,
                                Lesson = lesson
                            };
                            _context.TeacherLessons.Add(teacherLesson);
                        }
                    }

                    // Tambahkan relasi antara guru dan nomor unik ruang kelas (UniqueNumberOfClassRoom)
                    foreach (var uniqueNumber in teacherDto.UniqueNumberOfClassRoom)
                    {
                        var classroom = await _context.ClassRooms.FirstOrDefaultAsync(c => c.UniqueNumberOfClassRoom == uniqueNumber);
                        if (classroom != null)
                        {
                            var teacherClassroom = new TeacherClassRoom
                            {
                                Teacher = teacher,
                                ClassRoom = classroom
                            };
                            _context.TeacherClassRooms.Add(teacherClassroom);
                        }
                    }

                    await _context.SaveChangesAsync();

                    // Menggunakan metode CreateUserObjectTeacherGet untuk membuat objek TeacherDto
                    var teacherDtoResult = await CreateUserObjectTeacherGet(user);
                    return Result<TeacherRegisterDto>.Success(teacherDtoResult); // Mengembalikan hasil sukses dengan DTO guru
                }

                return Result<TeacherRegisterDto>.Failure(string.Join(",", result.Errors));
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
        }
    }
}