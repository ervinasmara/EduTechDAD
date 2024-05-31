using Application.Core;
using Application.User.DTOs.Edit;
using AutoMapper;
using Domain.Many_to_Many;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.User.Teachers.Command;
public class EditTeacher
{
    public class EditTeacherCommand : IRequest<Result<EditTeacherDto>>
    {
        public Guid TeacherId { get; set; }
        public EditTeacherDto TeacherDto { get; set; }
    }

    public class EditTeacherCommandHandler : IRequestHandler<EditTeacherCommand, Result<EditTeacherDto>>
    {
        private readonly DataContext _context;
        private readonly IMapper _mapper;

        public EditTeacherCommandHandler(DataContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<Result<EditTeacherDto>> Handle(EditTeacherCommand request, CancellationToken cancellationToken)
        {
            var teacherDto = request.TeacherDto;

            /** Langkah 1: Mencari guru berdasarkan ID **/
            var teacher = await _context.Teachers
                .Include(t => t.TeacherLessons) // Memuat relasi TeacherLessons
                .ThenInclude(tl => tl.Lesson) // Memuat relasi Lesson untuk setiap TeacherLesson
                .FirstOrDefaultAsync(t => t.Id == request.TeacherId);

            /** Langkah 2: Memeriksa apakah guru ditemukan **/
            if (teacher == null)
            {
                return Result<EditTeacherDto>.Failure("Guru tidak ditemukan");
            }

            /** Langkah 3: Memetakan data dari EditTeacherCommand ke entitas Teacher menggunakan AutoMapper **/
            _mapper.Map(request, teacher);

            /** Langkah 4: Dapatkan daftar pelajaran yang valid dari input guru **/
            var validLessons = await _context.Lessons
            .Include(tl => tl.TeacherLessons)
                .Where(l => teacherDto.LessonNames.Contains(l.LessonName))
                .ToListAsync(cancellationToken);

            /** Langkah 4.1: Pengecekan status pelajaran **/
            var invalidStatusLessons = validLessons.Where(l => l.Status == 0).ToList();
            if (invalidStatusLessons.Any())
            {
                var invalidStatusLessonNames = invalidStatusLessons.Select(l => l.LessonName).ToList();
                return Result<EditTeacherDto>.Failure($"Mapel {string.Join(", ", invalidStatusLessonNames)} sudah tidak tersedia");
            }

            /** Langkah 4.2: Melakukan validasi terhadap nama-nama pelajaran yang dipilih **/
            var invalidLessonNames = new List<string>();
            foreach (var lessonName in teacherDto.LessonNames)
            {
                var lesson = await _context.Lessons.FirstOrDefaultAsync(l => l.LessonName == lessonName);
                if (lesson == null)
                {
                    invalidLessonNames.Add(lessonName);
                }
            }

            // Jika ada nama pelajaran yang tidak valid
            if (invalidLessonNames.Any())
            {
                return Result<EditTeacherDto>.Failure($"Nama pelajaran {string.Join(", ", invalidLessonNames)} yang Anda pilih tidak ada");
            }


            /**Langkah 4.3: Validasi apakah semua mata pelajaran belum dimiliki oleh guru lain **/
            var existingTeacherLessons = await _context.TeacherLessons
                .Include(tl => tl.Lesson)
                .Include(tl => tl.Teacher)
                .Where(tl => teacherDto.LessonNames.Contains(tl.Lesson.LessonName) && tl.Teacher.Id != request.TeacherId && tl.Teacher.Status == 1)
                .ToListAsync(cancellationToken);

            if (existingTeacherLessons.Any())
            {
                var alreadyAssignedLessons = existingTeacherLessons
                    .Select(tl => new { tl.Lesson.LessonName, tl.Teacher.NameTeacher })
                    .ToList();

                var errorMessage = string.Join(", ", alreadyAssignedLessons
                    .Select(t => $"Pelajaran sudah dimiliki oleh guru {t.NameTeacher}, pada mapel {t.LessonName}"));

                return Result<EditTeacherDto>.Failure(errorMessage);
            }
            /** Langkah 5: Menghapus semua relasi pelajaran yang ada **/
            _context.TeacherLessons.RemoveRange(teacher.TeacherLessons);

            /** Langkah 6: Menambahkan kembali relasi pelajaran yang baru **/
            foreach (var lessonName in teacherDto.LessonNames)
            {
                var lesson = await _context.Lessons.FirstOrDefaultAsync(l => l.LessonName == lessonName);
                if (lesson != null)
                {
                    teacher.TeacherLessons.Add(new TeacherLesson { LessonId = lesson.Id });
                }
            }

            /** Langkah 7: Menyimpan perubahan ke database **/
            var result = await _context.SaveChangesAsync(cancellationToken) > 0;

            /** Langkah 8: Memeriksa hasil penyimpanan **/
            if (!result)
            {
                return Result<EditTeacherDto>.Failure("Gagal untuk edit Guru");
            }

            /** Langkah 9: Memetakan kembali entitas Teacher ke DTO **/
            var editedTeacherDto = _mapper.Map<EditTeacherDto>(teacher);
            editedTeacherDto.LessonNames = teacher.TeacherLessons.Select(tl => tl.Lesson.LessonName).ToList();

            /** Langkah 10: Mengembalikan hasil dalam bentuk Success Result dengan data guru yang diperbarui **/
            return Result<EditTeacherDto>.Success(editedTeacherDto);
        }
    }
}

public class EditTeacherCommandValidator : AbstractValidator<EditTeacherDto>
{
    public EditTeacherCommandValidator()
    {
        RuleFor(x => x.Address).NotEmpty().WithMessage("Alamat tidak boleh kosong");
        RuleFor(x => x.PhoneNumber)
            .NotEmpty().WithMessage("Nomor telepon tidak boleh kosong")
            .Matches("^[0-9\\-+]*$").WithMessage("Nomor telepon hanya boleh berisi angka, tanda minus (-), atau tanda plus (+).")
            .Length(8, 13).WithMessage("Nomor telepon harus terdiri dari 8 hingga 13 digit");
        RuleFor(x => x.LessonNames).NotEmpty().WithMessage("Nama pelajaran tidak boleh kosong");
    }
}