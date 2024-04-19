using Application.Core;
using MediatR;
using Persistence;
using Microsoft.EntityFrameworkCore;
using Application.User.DTOs;
using Domain.User;
using TeacherDomain = Domain.User.Teacher;
using Microsoft.AspNetCore.Identity;
using Application.User.DTOs.Registration;
using FluentValidation;
using Application.User.Validation;
using Domain.Many_to_Many;
using Domain.Class;
using Domain.Learn.Lessons;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

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

                var user = new AppUser
                {
                    UserName = teacherDto.Username,
                    Role = teacherDto.Role,
                };

                // Dapatkan semua Lesson yang valid dari input guru
                var validLessons = new List<Lesson>();
                foreach (var lessonName in teacherDto.LessonName)
                {
                    var lesson = await _context.Lessons.FirstOrDefaultAsync(l => l.LessonName == lessonName);
                    if (lesson == null)
                    {
                        return Result<TeacherRegisterDto>.Failure($"Lesson '{lessonName}' does not exist");
                    }
                    validLessons.Add(lesson);
                }

                // Dapatkan semua ClassName yang valid dari input guru
                var validClassRooms = new List<ClassRoom>();
                foreach (var lesson in validLessons)
                {
                    var lessonClassRooms = await _context.LessonClassRooms
                        .Where(lcr => lcr.LessonId == lesson.Id)
                        .Select(lcr => lcr.ClassRoom)
                        .ToListAsync();
                    foreach (var classRoom in lessonClassRooms)
                    {
                        validClassRooms.Add(classRoom);
                    }
                }

                foreach (var className in teacherDto.ClassNames)
                {
                    var classRoom = validClassRooms.FirstOrDefault(c => c.ClassName == className);
                    if (classRoom == null)
                    {
                        var lessonNames = validLessons.Select(l => l.LessonName);
                        return Result<TeacherRegisterDto>.Failure($"Classroom with '{className}' does not exist for any of the lessons: {string.Join(", ", lessonNames)}");
                    }
                }

                // Check if the selected lesson and classroom combination is already assigned to another teacher
                var conflictingAssignments = await _context.TeacherClassRooms
                    .Include(tcr => tcr.Teacher)
                    .Include(tcr => tcr.ClassRoom)
                    .Include(tcr => tcr.Teacher.TeacherLessons) // Include TeacherLessons
                    .Where(tcr =>
                        tcr.Teacher.NameTeacher != teacherDto.NameTeacher && // Exclude the current teacher
                        tcr.Teacher.TeacherLessons.Any(tl => teacherDto.LessonName.Contains(tl.Lesson.LessonName)) && // Check if any of the lessons is already assigned
                        teacherDto.ClassNames.Contains(tcr.ClassRoom.ClassName)) // Check if the class is among selected classrooms
                    .ToListAsync();

                if (conflictingAssignments.Any())
                {
                    var conflictMessages = conflictingAssignments
                        .Select(ca => $"Classroom '{ca.ClassRoom.ClassName}' for one of the selected lessons is already assigned to teacher '{ca.Teacher.NameTeacher}'");
                    return Result<TeacherRegisterDto>.Failure(string.Join("; ", conflictMessages));
                }

                // Buat objek guru
                var teacher = new TeacherDomain
                {
                    NameTeacher = teacherDto.NameTeacher,
                    BirthDate = teacherDto.BirthDate,
                    BirthPlace = teacherDto.BirthPlace,
                    Address = teacherDto.Address,
                    PhoneNumber = teacherDto.PhoneNumber,
                    Nip = teacherDto.Nip,
                    AppUserId = user.Id
                };

                // Simpan guru ke dalam konteks database
                _context.Teachers.Add(teacher);

                // Tambahkan relasi antara guru dan pelajaran (Lesson) ke tabel pivot TeacherLesson
                foreach (var lesson in validLessons)
                {
                    // Ambil kelas yang dipilih oleh guru untuk pelajaran ini
                    var selectedClassRoomsForLesson = teacherDto.ClassNames
                        .Select(className => validClassRooms.FirstOrDefault(c => c.ClassName == className))
                        .Where(classRoom => classRoom != null)
                        .ToList();

                    // Pastikan setidaknya ada satu kelas yang dipilih oleh guru untuk pelajaran ini
                    if (selectedClassRoomsForLesson.Count == 0)
                    {
                        return Result<TeacherRegisterDto>.Failure($"No class selected for the lesson '{lesson.LessonName}'");
                    }

                    // Tambahkan relasi antara guru dan kelas yang dipilih untuk pelajaran ini
                    foreach (var classRoom in selectedClassRoomsForLesson)
                    {
                        var teacherClassroom = new TeacherClassRoom
                        {
                            Teacher = teacher,
                            ClassRoom = classRoom
                        };
                        _context.TeacherClassRooms.Add(teacherClassroom);

                        // Tambahkan relasi antara guru dan pelajaran ke tabel pivot TeacherLesson
                        var teacherLesson = new TeacherLesson
                        {
                            Teacher = teacher,
                            Lesson = lesson
                        };
                        _context.TeacherLessons.Add(teacherLesson);
                    }
                }

                var result = await _userManager.CreateAsync(user, teacherDto.Password);

                if (result.Succeeded)
                {
                    await _context.SaveChangesAsync();

                    // Menggunakan metode CreateUserObjectTeacherGet untuk membuat objek TeacherDto
                    var teacherDtoResult = await CreateUserObjectTeacherGet(user);
                    return Result<TeacherRegisterDto>.Success(teacherDtoResult); // Mengembalikan hasil sukses dengan DTO guru
                }

                // Jika pembuatan pengguna gagal, maka batalkan penyimpanan guru dan kembalikan hasil error
                _context.Teachers.Remove(teacher);
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
