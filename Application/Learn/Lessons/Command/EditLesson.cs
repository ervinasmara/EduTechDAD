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

                /** Langkah 3: Update properti Lesson **/
                lesson.LessonName = request.LessonCreateAndEditDto.LessonName;
                lesson.ClassRoomId = classroom.Id;

                /** Langkah 4: Simpan perubahan ke database **/
                var result = await _context.SaveChangesAsync(cancellationToken) > 0;

                /** Langkah 5: Kirimkan DTO dalam Response **/
                /** Langkah 5.1: Map Lesson ke LessonDto **/
                var lessonDto = _mapper.Map<LessonCreateAndEditDto>(lesson);

                /** Langkah 5.2: Atur properti ClassName di LessonDto **/
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