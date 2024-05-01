using Application.Core;
using AutoMapper;
using MediatR;
using Persistence;
using Microsoft.EntityFrameworkCore;
using Application.Interface;
using FluentValidation;

namespace Application.Learn.Courses.Command
{
    public class EditCourse
    {
        public class Command : IRequest<Result<CourseCreateAndEditDto>>
        {
            public Guid CourseId { get; set; }
            public CourseCreateAndEditDto CourseCreateAndEditDto { get; set; }
        }

        public class CommandValidatorDto : AbstractValidator<Command>
        {
            public CommandValidatorDto()
            {
                RuleFor(x => x.CourseCreateAndEditDto.CourseName).NotEmpty();
                RuleFor(x => x.CourseCreateAndEditDto.Description).NotEmpty();
                RuleFor(x => x.CourseCreateAndEditDto.LessonName).NotEmpty();

                // Validasi untuk memastikan bahwa setidaknya satu dari LinkCourse diisi
                RuleFor(x => x.CourseCreateAndEditDto.LinkCourse)
                    .NotEmpty()
                    .When(x => x.CourseCreateAndEditDto.FileData == null) // Hanya memeriksa LinkCourse jika FileData kosong
                    .WithMessage("LinkCourse must be provided if FileData is not provided.");
            }
        }

        public class Handler : IRequestHandler<Command, Result<CourseCreateAndEditDto>>
        {
            private readonly DataContext _context;
            private readonly IUserAccessor _userAccessor;
            private readonly IMapper _mapper;
            private readonly IFileService _fileService;

            public Handler(DataContext context, IUserAccessor userAccessor, IMapper mapper, IFileService fileService)
            {
                _context = context;
                _userAccessor = userAccessor;
                _mapper = mapper;
                _fileService = fileService;
            }

            public async Task<Result<CourseCreateAndEditDto>> Handle(Command request, CancellationToken cancellationToken)
            {
                /** Langkah 1: Memeriksa teacherId **/
                var teacherId = _userAccessor.GetTeacherIdFromToken();
                if (teacherId == null)
                {
                    return Result<CourseCreateAndEditDto>.Failure("Teacher ID not found in token");
                }

                /** Langkah 2: Memeriksa LessonName dan mendapatkan LessonId **/
                var lesson = await _context.Lessons
                    .Include(tl => tl.TeacherLessons)
                    .FirstOrDefaultAsync(x => x.LessonName == request.CourseCreateAndEditDto.LessonName);

                // Memeriksa apakah lesson yang dimasukkan ada didatabase
                if (lesson == null)
                {
                    return Result<CourseCreateAndEditDto>.Failure($"Lesson with LessonName {request.CourseCreateAndEditDto.LessonName} not found.");
                }

                // Memeriksa apakah teacher memiliki keterkaitan dengan lesson yang dimasukkan
                if (lesson.TeacherLessons == null || !lesson.TeacherLessons.Any(tl => tl.TeacherId == Guid.Parse(teacherId)))
                {
                    return Result<CourseCreateAndEditDto>.Failure($"Teacher does not have this lesson.");
                }

                /** Langkah 3: Membuat entity Course dari DTO **/
                var course = await _context.Courses.FindAsync(request.CourseId);

                if (course == null)
                {
                    return Result<CourseCreateAndEditDto>.Failure("Course not found");
                }

                /** Langkah 4: Menyimpan file jika ada **/
                string filePath = null;
                if (request.CourseCreateAndEditDto.FileData != null)
                {
                    string relativeFolderPath = "Upload/FileCourse";
                    filePath = await _fileService.SaveFileAsync(request.CourseCreateAndEditDto.FileData, relativeFolderPath, request.CourseCreateAndEditDto.CourseName, course.CreatedAt);
                }

                // Setelah menyimpan file, set FilePath pada course
                course.FilePath = filePath;

                // Membuat Mapper untuk edit course
                _mapper.Map(request.CourseCreateAndEditDto, course, opts =>
                {
                    opts.Items["Lesson"] = lesson; // Menambahkan Lesson ke context untuk digunakan dalam AfterMap
                });

                /** Langkah 5: Menyimpan Course ke database **/
                var result = await _context.SaveChangesAsync() > 0;
                if (!result)
                {
                    return Result<CourseCreateAndEditDto>.Failure("Failed to create course.");
                }

                /** Langkah 6: Mengembalikan hasil **/
                var courseDto = _mapper.Map<CourseCreateAndEditDto>(course);
                courseDto.FileData = request.CourseCreateAndEditDto.FileData; // Menambahkan FileData ke DTO
                courseDto.LessonName = lesson?.LessonName; // Menambahkan LessonName ke DTO

                return Result<CourseCreateAndEditDto>.Success(courseDto);
            }
        }
    }
}