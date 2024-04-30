using Application.Core;
using Application.Interface.User;
using Application.User.DTOs.Registration;
using AutoMapper;
using Domain.Many_to_Many;
using Domain.User;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Infrastructure.User
{
    public class UserTeacher : IUserTeacher
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly IMapper _mapper;
        private readonly DataContext _context;

        public UserTeacher(UserManager<AppUser> userManager, IMapper mapper, DataContext context)
        {
            _userManager = userManager;
            _mapper = mapper;
            _context = context;
        }

        public async Task<Result<RegisterTeacherDto>> CreateTeacherAsync(RegisterTeacherDto teacherDto, CancellationToken cancellationToken)
        {
            // Pemeriksaan username supaya berbeda dengan yang lain
            if (await _userManager.Users.AnyAsync(x => x.UserName == teacherDto.Username, cancellationToken))
            {
                return Result<RegisterTeacherDto>.Failure("Username already in use");
            }

            // Pemeriksaan NIP supaya berbeda dengan yang lain
            if (await _context.Teachers.AnyAsync(t => t.Nip == teacherDto.Nip, cancellationToken))
            {
                return Result<RegisterTeacherDto>.Failure("NIP already in use");
            }

            // Dapatkan pelajaran yang valid dari input guru
            var validLessons = await _context.Lessons
                .Where(l => teacherDto.LessonNames.Contains(l.LessonName))
                .ToListAsync(cancellationToken);

            // Jika ada nama pelajaran yang tidak valid, gagalkan proses
            if (validLessons.Count != teacherDto.LessonNames.Count)
            {
                var invalidLessonNames = teacherDto.LessonNames.Except(validLessons.Select(l => l.LessonName)).ToList();
                return Result<RegisterTeacherDto>.Failure($"Invalid lesson names: {string.Join(", ", invalidLessonNames)}");
            }

            // Jika semua validasi berhasil, buat AppUser baru
            var user = new AppUser
            {
                UserName = teacherDto.Username,
                Role = 2, // Asumsi: Role 2 adalah untuk guru
            };

            var createUserResult = await _userManager.CreateAsync(user, teacherDto.Password);
            if (!createUserResult.Succeeded)
            {
                return Result<RegisterTeacherDto>.Failure(string.Join(",", createUserResult.Errors));
            }

            // Pemetaan dari RegisterTeacherDto ke Teacher
            var teacher = _mapper.Map<Teacher>(teacherDto);
            teacher.AppUserId = user.Id;

            // Tambahkan relasi pelajaran yang valid
            foreach (var lesson in validLessons)
            {
                teacher.TeacherLessons.Add(new TeacherLesson { Lesson = lesson });
            }

            // Tambahkan teacher ke context dan simpan perubahan
            _context.Teachers.Add(teacher);
            await _context.SaveChangesAsync(cancellationToken);

            // Pemetaan dari Teacher kembali ke RegisterTeacherDto
            var teacherDtoResult = _mapper.Map<RegisterTeacherDto>(teacher);
            teacherDtoResult.LessonNames = validLessons.Select(l => l.LessonName).ToList();

            return Result<RegisterTeacherDto>.Success(teacherDtoResult);
        }
    }
}
