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
                    // Temukan semua guru berdasarkan nama yang diberikan
                    var teachers = await _context.Teachers
                        .Where(t => request.LessonCreateDto.NameTeachers.Contains(t.NameTeacher))
                        .ToListAsync();

                    if (teachers.Count != request.LessonCreateDto.NameTeachers.Count)
                    {
                        var missingTeachers = request.LessonCreateDto.NameTeachers.Except(teachers.Select(t => t.NameTeacher));
                        return Result<LessonCreateDto>.Failure($"Teachers with names {string.Join(", ", missingTeachers)} not found.");
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

                    foreach (var teacher in teachers)
                    {
                        _context.TeacherLessons.Add(new TeacherLesson
                        {
                            TeacherId = teacher.Id,
                            LessonId = lesson.Id
                        });
                    }

                    var result = await _context.SaveChangesAsync(cancellationToken) > 0;

                    if (!result)
                        return Result<LessonCreateDto>.Failure("Failed to create Lesson");

                    var lessonDto = _mapper.Map<LessonCreateDto>(lesson);
                    lessonDto.NameTeachers = teachers.Select(t => t.NameTeacher).ToList(); // Mengisi NameTeachers dalam DTO dengan koleksi nama guru

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