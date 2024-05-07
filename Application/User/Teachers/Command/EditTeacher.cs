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

            /** Langkah 4: Melakukan validasi terhadap nama-nama pelajaran yang dipilih **/
            foreach (var lessonName in request.TeacherDto.LessonNames)
            {
                var lesson = await _context.Lessons.FirstOrDefaultAsync(l => l.LessonName == lessonName);
                if (lesson == null)
                {
                    return Result<EditTeacherDto>.Failure($"Nama pelajaran '{lessonName}' yang Anda pilih tidak ada");
                }
            }

            /** Langkah 5: Menghapus semua relasi pelajaran yang ada **/
            _context.TeacherLessons.RemoveRange(teacher.TeacherLessons);

            /** Langkah 6: Menambahkan kembali relasi pelajaran yang baru **/
            foreach (var lessonName in request.TeacherDto.LessonNames)
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