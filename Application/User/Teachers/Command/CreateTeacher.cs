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
                return Result<RegisterTeacherDto>.Failure("Username sudah digunakan");
            }

            /** Langkah 2: Periksa apakah NIP sudah digunakan **/
            if (await _context.Teachers.AnyAsync(t => t.Nip == teacherDto.Nip, cancellationToken))
            {
                return Result<RegisterTeacherDto>.Failure("NIP sudah digunakan");
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
                return Result<RegisterTeacherDto>.Failure($"Nama pelajaran tidak valid: {string.Join(", ", invalidLessonNames)}");
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
        RuleFor(x => x.NameTeacher).NotEmpty().WithMessage("Nama tidak boleh kosong");
        RuleFor(x => x.BirthDate).NotEmpty().WithMessage("Tanggal lahir tidak boleh kosong");
        RuleFor(x => x.BirthPlace).NotEmpty().WithMessage("Tempat lahir tidak boleh kosong");
        RuleFor(x => x.Address).NotEmpty().WithMessage("Alamat tidak boleh kosong");
        RuleFor(x => x.PhoneNumber)
            .NotEmpty().WithMessage("Nomor telepon tidak boleh kosong")
            .Matches("^[0-9]*$").WithMessage("Nomor telepon hanya boleh berisi angka")
            .Length(8, 13).WithMessage("Nomor telepon harus terdiri dari 8 hingga 13 digit")
            .Must(phone => phone.StartsWith("0")).WithMessage("Nomor telepon harus diawali dengan angka 0");
        RuleFor(x => x.Nip).NotEmpty().WithMessage("Nip tidak boleh kosong");
        RuleFor(x => x.Gender)
            .NotEmpty().WithMessage("Jenis kelamin tidak boleh kosong")
            .Must(gender => gender >= 1 && gender <= 2)
            .WithMessage("Nilai Jenis kelamin tidak valid. Gunakan 1 untuk Laki-laki, 2 untuk Perempuan");
        RuleFor(x => x.Username)
            .NotEmpty().WithMessage("Nama pengguna tidak boleh kosong")
            .Length(5, 20).WithMessage("Panjang nama pengguna harus antara 5 hingga 20 karakter")
            .Must(x => !x.Contains(" ")).WithMessage("Nama pengguna tidak boleh mengandung spasi");
        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Kata sandi tidak boleh kosong")
            .Matches("(?=.*\\d)(?=.*[a-z])(?=.*[A-Z])(?!.*\\s).{8,16}")
            .WithMessage("Kata sandi harus memenuhi kriteria berikut: minimal 8 karakter, maksimal 16 karakter, setidaknya satu huruf kecil, satu huruf besar, dan satu angka");
        RuleFor(x => x.LessonNames).NotEmpty().WithMessage("Nama Pelajaran tidak boleh kosong");
    }
}