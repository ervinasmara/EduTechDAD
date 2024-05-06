using Application.Core;
using AutoMapper;
using Domain.Learn.Courses;
using FluentValidation;
using MediatR;
using Persistence;
using Microsoft.EntityFrameworkCore;
using Application.Interface;

namespace Application.Learn.Courses.Command;
public class CreateCourse
{
    public class Command : IRequest<Result<CourseCreateAndEditDto>>
    {
        public CourseCreateAndEditDto CourseCreateAndEditDto { get; set; }
    }

    public class CommandValidatorDto : AbstractValidator<Command>
    {
        public CommandValidatorDto()
        {
            RuleFor(x => x.CourseCreateAndEditDto).SetValidator(new CourseCreateAndEditValidator());
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
            var course = _mapper.Map<Course>(request.CourseCreateAndEditDto, opts =>
            {
                opts.Items["Lesson"] = lesson; // Menambahkan Lesson ke context untuk digunakan dalam AfterMap
            });

            /** Langkah 4: Menyimpan file jika ada **/
            string filePath = null;
            if (request.CourseCreateAndEditDto.FileData != null)
            {
                string relativeFolderPath = "Upload/FileCourse";
                filePath = await _fileService.SaveFileAsync(request.CourseCreateAndEditDto.FileData, relativeFolderPath, request.CourseCreateAndEditDto.CourseName, course.CreatedAt);
            }

            if (request.CourseCreateAndEditDto.FileData != null)
            {
                string fileExtension = Path.GetExtension(request.CourseCreateAndEditDto.FileData.FileName);
                if (!string.Equals(fileExtension, ".pdf", StringComparison.OrdinalIgnoreCase))
                {
                    return Result<CourseCreateAndEditDto>.Failure("Only PDF files are allowed.");
                }
            }

            // Setelah menyimpan file, set FilePath pada course
            course.FilePath = filePath;

            // Menyesuaikan CreatedAt dengan waktu Indonesia
            course.CreatedAt = DateTime.UtcNow.AddHours(7);

            /** Langkah 5: Menyimpan Course ke database **/
            _context.Courses.Add(course);
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