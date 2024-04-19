using MediatR;
using Persistence;
using FluentValidation;
using Application.Core;
using AutoMapper;
using Domain.Learn.Lessons;
using Microsoft.EntityFrameworkCore;
using Domain.Many_to_Many;

namespace Application.Learn.Lessons
{
    public class Create
    {
        public class Command : IRequest<Result<LessonCreateDto>>
        {
            public LessonCreateDto LessonCreateDto { get; set; }
        }

        public class CommandLesson : AbstractValidator<Command>
        {
            public CommandLesson()
            {
                RuleFor(x => x.LessonCreateDto).SetValidator(new LessonCreateValidator());
            }
        }

        public class Handler : IRequestHandler<Command, Result<LessonCreateDto>>
        {
            private readonly DataContext _context;
            private readonly IMapper _mapper;

            public Handler(DataContext context, IMapper mapper)
            {
                _context = context;
                _mapper = mapper;
            }

            public async Task<Result<LessonCreateDto>> Handle(Command request, CancellationToken cancellationToken)
            {
                try
                {
                    // Temukan semua kelas berdasarkan nama yang diberikan
                    var classrooms = await _context.ClassRooms
                        .Where(c => request.LessonCreateDto.ClassNames.Contains(c.ClassName))
                        .ToListAsync();

                    if (classrooms.Count != request.LessonCreateDto.ClassNames.Count)
                    {
                        var missingClassrooms = request.LessonCreateDto.ClassNames.Except(classrooms.Select(c => c.ClassName));
                        return Result<LessonCreateDto>.Failure($"Classrooms with names {string.Join(", ", missingClassrooms)} not found.");
                    }

                    var lastLesson = await _context.Lessons
                        .OrderByDescending(x => x.UniqueNumberOfLesson)
                        .FirstOrDefaultAsync(cancellationToken);

                    int newUniqueNumber = (lastLesson != null) ? int.Parse(lastLesson.UniqueNumberOfLesson) + 1 : 1;
                    var uniqueNumber = newUniqueNumber.ToString("00");

                    var lesson = new Lesson
                    {
                        LessonName = request.LessonCreateDto.LessonName,
                        UniqueNumberOfLesson = uniqueNumber
                    };

                    _context.Lessons.Add(lesson);

                    foreach (var classroom in classrooms)
                    {
                        _context.LessonClassRooms.Add(new LessonClassRoom
                        {
                            LessonId = lesson.Id,
                            ClassRoomId = classroom.Id
                        });
                    }

                    var result = await _context.SaveChangesAsync(cancellationToken) > 0;

                    if (!result)
                        return Result<LessonCreateDto>.Failure("Failed to create Lesson");

                    var lessonDto = _mapper.Map<LessonCreateDto>(lesson);
                    lessonDto.ClassNames = classrooms.Select(c => c.ClassName).ToList(); // Mengisi ClassNames dalam DTO dengan koleksi nama kelas

                    return Result<LessonCreateDto>.Success(lessonDto);
                }
                catch (Exception ex)
                {
                    return Result<LessonCreateDto>.Failure($"Failed to create lesson: {ex.Message}");
                }
            }
        }
    }
}