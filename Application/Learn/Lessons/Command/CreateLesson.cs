using MediatR;
using Persistence;
using FluentValidation;
using Application.Core;
using AutoMapper;
using Domain.Learn.Lessons;
using Microsoft.EntityFrameworkCore;

namespace Application.Learn.Lessons.Command;
public class CreateLesson
{
    public class Command : IRequest<Result<LessonCreateAndEditDto>>
    {
        public LessonCreateAndEditDto LessonCreateAndEditDto { get; set; }
    }

    public class CommandLesson : AbstractValidator<Command>
    {
        public CommandLesson()
        {
            RuleFor(x => x.LessonCreateAndEditDto).SetValidator(new LessonCreateAndEditValidator());
        }
    }

    public class Handler : IRequestHandler<Command, Result<LessonCreateAndEditDto>>
    {
        private readonly DataContext _context;
        private readonly IMapper _mapper;

        public Handler(DataContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<Result<LessonCreateAndEditDto>> Handle(Command request, CancellationToken cancellationToken)
        {
            /** Langkah 1: Temukan Kelas berdasarkan Nama **/
            var classroom = await _context.ClassRooms
                .FirstOrDefaultAsync(c => c.ClassName == request.LessonCreateAndEditDto.ClassName);

            if (classroom == null)
            {
                return Result<LessonCreateAndEditDto>.Failure($"Classroom dengan nama '{request.LessonCreateAndEditDto.ClassName}' tidak ditemukan");
            }

            /** Langkah 2: Validasi Keunikan Nama Pelajaran **/
            var isLessonNameUnique = await _context.Lessons
                .AllAsync(l => l.LessonName != request.LessonCreateAndEditDto.LessonName);

            if (!isLessonNameUnique)
            {
                return Result<LessonCreateAndEditDto>.Failure("Nama mapel harus unik");
            }

            /** Langkah 3: Tentukan UniqueNumberOfLesson **/
            /** Langkah 3.1: Dapatkan Lesson terakhir (diurutkan berdasarkan UniqueNumberOfLesson) **/
            var lastLesson = await _context.Lessons
                .OrderByDescending(x => x.UniqueNumberOfLesson)
                .FirstOrDefaultAsync(cancellationToken);

            /** Langkah 3.2: Hitung UniqueNumberOfLesson baru **/
            int newUniqueNumber = lastLesson != null ? int.Parse(lastLesson.UniqueNumberOfLesson) + 1 : 1;
            var uniqueNumber = newUniqueNumber.ToString("00"); // Format menjadi "00" (misal: 01, 02, dst)

            /** Langkah 4: Buat objek Lesson dari DTO dan Atur properti **/
            var lesson = _mapper.Map<Lesson>(request.LessonCreateAndEditDto, opts => opts.Items["ClassRoom"] = classroom);
            lesson.UniqueNumberOfLesson = uniqueNumber;

            /** Langkah 5: Simpan Lesson ke Database **/
            _context.Lessons.Add(lesson);

            var result = await _context.SaveChangesAsync(cancellationToken) > 0;

            if (!result)
                return Result<LessonCreateAndEditDto>.Failure("Gagal untuk membuat Lesson");

            /** Langkah 6: Kirimkan DTO dalam Response **/
            var lessonDto = _mapper.Map<LessonCreateAndEditDto>(lesson);
            return Result<LessonCreateAndEditDto>.Success(lessonDto);
        }
    }
}