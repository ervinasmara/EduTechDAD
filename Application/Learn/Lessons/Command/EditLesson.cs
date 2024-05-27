using MediatR;
using Persistence;
using AutoMapper;
using FluentValidation;
using Application.Core;
using Microsoft.EntityFrameworkCore;

namespace Application.Learn.Lessons.Command;
public class EditLesson
{
    public class Command : IRequest<Result<LessonCreateAndEditDto>>
    {
        public Guid LessonId { get; set; } // Id lesson yang akan diedit
        public LessonCreateAndEditDto LessonCreateAndEditDto { get; set; } // Data baru untuk lesson
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
            try
            {
                /** Langkah 1: Temukan Lesson berdasarkan LessonId **/
                var lesson = await _context.Lessons
                    .FirstOrDefaultAsync(l => l.Id == request.LessonId, cancellationToken);

                if (lesson == null)
                {
                    return Result<LessonCreateAndEditDto>.Failure("Pelajaran tidak ditemukan");
                }

                /** Langkah 2: Temukan Classroom berdasarkan ClassName **/
                /** Langkah 2.1: Ambil ClassName dari DTO **/
                var requestedClassName = request.LessonCreateAndEditDto.ClassName;

                /** Langkah 2.2: Cari objek ClassRoom berdasarkan ClassName **/
                var classroom = await _context.ClassRooms
                    .FirstOrDefaultAsync(c => c.ClassName == requestedClassName, cancellationToken);

                if (classroom == null)
                {
                    return Result<LessonCreateAndEditDto>.Failure($"Kelas dengan nama '{requestedClassName}' tidak ditemukan");
                }

                /** Langkah 3: Validasi Keunikan Nama Pelajaran **/
                var lessonNameWithClass = $"{request.LessonCreateAndEditDto.LessonName} - {requestedClassName}";
                var isLessonNameUnique = await _context.Lessons
                    .AllAsync(l => l.Id == request.LessonId || l.LessonName != lessonNameWithClass, cancellationToken);

                if (!isLessonNameUnique)
                {
                    return Result<LessonCreateAndEditDto>.Failure("Nama mapel harus unik");
                }

                /** Langkah 4: Periksa apakah ada perubahan **/
                if (lesson.LessonName == lessonNameWithClass && lesson.ClassRoomId == classroom.Id)
                {
                    return Result<LessonCreateAndEditDto>.Failure("Tidak ada perubahan yang dilakukan pada pelajaran");
                }

                /** Langkah 5: Update properti Lesson **/
                lesson.LessonName = lessonNameWithClass;
                lesson.ClassRoomId = classroom.Id;

                /** Langkah 6: Simpan perubahan ke database **/
                var result = await _context.SaveChangesAsync(cancellationToken) > 0;

                if (!result)
                {
                    return Result<LessonCreateAndEditDto>.Failure("Gagal untuk mengedit pelajaran");
                }

                /** Langkah 7: Kirimkan DTO dalam Response **/
                var lessonDto = _mapper.Map<LessonCreateAndEditDto>(lesson);
                lessonDto.ClassName = classroom.ClassName;

                return Result<LessonCreateAndEditDto>.Success(lessonDto);
            }
            catch (Exception ex)
            {
                return Result<LessonCreateAndEditDto>.Failure($"Gagal untuk mengedit pelajaran: {ex.Message}");
            }
        }
    }
}