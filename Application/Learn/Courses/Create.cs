using Application.Core;
using AutoMapper;
using Domain.Learn.Courses;
using FluentValidation;
using MediatR;
using Persistence;

namespace Application.Learn.Courses
{
    public class Create
    {
        public class Command : IRequest<Result<CourseDto>>
        {
            public CourseDto CourseDto { get; set; }
        }

        public class CommandValidatorDto : AbstractValidator<Command>
        {
            public CommandValidatorDto()
            {
                RuleFor(x => x.CourseDto).SetValidator(new CourseValidator());
            }
        }

        public class Handler : IRequestHandler<Command, Result<CourseDto>>
        {
            private readonly DataContext _context;
            private readonly IMapper _mapper;

            public Handler(DataContext context, IMapper mapper)
            {
                _context = context;
                _mapper = mapper;
            }

            public async Task<Result<CourseDto>> Handle(Command request, CancellationToken cancellationToken)
            {
                try
                {
                    // Membaca data file jika CourseDto.FileData tidak null
                    byte[]? fileData = null;
                    if (request.CourseDto.FileData != null)
                    {
                        using (var memoryStream = new MemoryStream())
                        {
                            await request.CourseDto.FileData.CopyToAsync(memoryStream);
                            fileData = memoryStream.ToArray();
                        }
                    }

                    // Menemukan Lesson yang sesuai berdasarkan UniqueNumberOfLesson
                    var lesson = _context.Lessons.FirstOrDefault(x => x.UniqueNumberOfLesson == request.CourseDto.UniqueNumberOfLesson);
                    if (lesson == null)
                        return Result<CourseDto>.Failure($"Lesson with UniqueNumberOfLesson {request.CourseDto.UniqueNumberOfLesson} not found.");

                    // Membuat objek Course dari data yang diterima
                    var course = new Course
                    {
                        CourseName = request.CourseDto.CourseName,
                        Description = request.CourseDto.Description,
                        FileData = fileData,
                        LinkCourse = request.CourseDto.LinkCourse,
                        LessonId = lesson.Id // Mengisi LessonId dengan Id Lesson yang sesuai
                    };

                    // Menambahkan Course ke database
                    _context.Courses.Add(course);
                    await _context.SaveChangesAsync(cancellationToken);

                    // Mengembalikan CourseDto yang baru dibuat
                    var courseDto = _mapper.Map<CourseDto>(course);
                    courseDto.UniqueNumberOfLesson = lesson.UniqueNumberOfLesson;
                    return Result<CourseDto>.Success(courseDto);
                }
                catch (Exception ex)
                {
                    // Menangani kesalahan dan mengembalikan pesan kesalahan yang sesuai
                    return Result<CourseDto>.Failure($"Failed to save file: {ex.Message}");
                }
            }
        }
    }
}