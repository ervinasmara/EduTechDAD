using Application.Core;
using MediatR;
using Persistence;
using Microsoft.EntityFrameworkCore;
using Domain.User;
using Microsoft.AspNetCore.Identity;
using Application.User.DTOs.Registration;
using FluentValidation;
using Domain.Many_to_Many;
using AutoMapper;

namespace Application.User.Teachers.Command;
public class CreateTeacher
{
    public class RegisterTeacherCommand : IRequest<Result<RegisterTeacherDto>>
    {
        public RegisterTeacherDto TeacherDto { get; set; }
    }

    public class RegisterTeacherCommandHandler : IRequestHandler<RegisterTeacherCommand, Result<RegisterTeacherDto>>
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly DataContext _context;
        private readonly IMapper _mapper;

        public RegisterTeacherCommandHandler(UserManager<AppUser> userManager, DataContext context, IMapper mapper)
        {
            _userManager = userManager;
            _context = context;
            _mapper = mapper;
        }

        public async Task<Result<RegisterTeacherDto>> Handle(RegisterTeacherCommand request, CancellationToken cancellationToken)
        {
            var teacherDto = request.TeacherDto;

            /** Langkah 1: Periksa apakah username sudah digunakan **/
            if (await _userManager.Users.AnyAsync(x => x.UserName == teacherDto.Username, cancellationToken))
            {
                return Result<RegisterTeacherDto>.Failure("Username already in use");
            }

            /** Langkah 2: Periksa apakah NIP sudah digunakan **/
            if (await _context.Teachers.AnyAsync(t => t.Nip == teacherDto.Nip, cancellationToken))
            {
                return Result<RegisterTeacherDto>.Failure("NIP already in use");
            }

            /** Langkah 3: Dapatkan daftar pelajaran yang valid dari input guru **/
            var validLessons = await _context.Lessons
                .Include(tl => tl.TeacherLessons)
                .Where(l => teacherDto.LessonNames.Contains(l.LessonName))
                .ToListAsync(cancellationToken);

            /** Langkah 4: Periksa apakah semua nama pelajaran yang dimasukkan valid **/
            if (validLessons.Count != teacherDto.LessonNames.Count)
            {
                var invalidLessonNames = teacherDto.LessonNames.Except(validLessons.Select(l => l.LessonName)).ToList();
                return Result<RegisterTeacherDto>.Failure($"Invalid lesson names: {string.Join(", ", invalidLessonNames)}");
            }

            /** Langkah 5: Buat AppUser baru jika semua validasi berhasil **/
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

            /** Langkah 6: Pemetaan dari RegisterTeacherDto ke Teacher **/
            var teacher = _mapper.Map<Teacher>(teacherDto);
            teacher.AppUserId = user.Id;

            teacher.TeacherLessons = new List<TeacherLesson>(); // Inisialisasi koleksi

            /** Langkah 7: Tambahkan relasi pelajaran yang valid **/
            foreach (var lesson in validLessons)
            {
                teacher.TeacherLessons.Add(new TeacherLesson { Lesson = lesson });
            }

            /** Langkah 8: Tambahkan teacher ke context dan simpan perubahan **/
            _context.Teachers.Add(teacher);
            await _context.SaveChangesAsync(cancellationToken);

            /** Langkah 9: Pemetaan dari Teacher kembali ke RegisterTeacherDto **/
            var teacherDtoResult = _mapper.Map<RegisterTeacherDto>(teacher);
            teacherDtoResult.LessonNames = validLessons.Select(l => l.LessonName).ToList();

            /** Langkah 10: Kembalikan hasil yang berhasil **/
            return Result<RegisterTeacherDto>.Success(teacherDtoResult);
        }
    }
}

public class RegisterTeacherCommandValidator : AbstractValidator<RegisterTeacherDto>
{
    public RegisterTeacherCommandValidator()
    {
        RuleFor(x => x.NameTeacher).NotEmpty().WithMessage("Name is required.");
        RuleFor(x => x.BirthDate).NotEmpty().WithMessage("BirthDate is required.");
        RuleFor(x => x.BirthPlace).NotEmpty().WithMessage("BirthPlace is required.");
        RuleFor(x => x.Address).NotEmpty().WithMessage("Address is required.");
        RuleFor(x => x.PhoneNumber).NotEmpty().WithMessage("Phone number is required.")
                                      .Matches("^[0-9]{8,13}$").WithMessage("Phone number must be between 8 and 13 digits and contain only numbers.");
        RuleFor(x => x.Nip).NotEmpty().WithMessage("Nip is required.");
        RuleFor(x => x.Gender).NotEmpty().WithMessage("Gender is required.")
                              .InclusiveBetween(1, 2).WithMessage("Gender must be 1 for Male or 2 for Female.");
        RuleFor(x => x.Username).NotEmpty().WithMessage("Username is required.");
        RuleFor(x => x.Password)
            .NotEmpty()
            .Matches("(?=.*\\d)(?=.*[a-z])(?=.*[A-Z])(?!.*\\s).{8,16}")
            .WithMessage("Password must be complex");

        RuleFor(x => x.LessonNames).NotEmpty().WithMessage("LessonNames is required.");
    }
}