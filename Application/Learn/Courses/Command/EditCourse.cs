using Application.Core;
using AutoMapper;
using MediatR;
using Persistence;
using Microsoft.EntityFrameworkCore;
using Application.Interface;
using FluentValidation;

namespace Application.Learn.Courses.Command;
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
                return Result<CourseCreateAndEditDto>.Failure("TeacherId tidak ditemukan ditoken");
            }

            /** Langkah 2: Memeriksa LessonName dan mendapatkan LessonId **/
            var lesson = await _context.Lessons
                .Include(tl => tl.TeacherLessons)
                .FirstOrDefaultAsync(x => x.LessonName == request.CourseCreateAndEditDto.LessonName);

            // Memeriksa apakah lesson yang dimasukkan ada didatabase
            if (lesson == null)
            {
                return Result<CourseCreateAndEditDto>.Failure($"Mapel dengan nama mapel {request.CourseCreateAndEditDto.LessonName} tidak ditemukan");
            }

            // Memeriksa apakah teacher memiliki keterkaitan dengan lesson yang dimasukkan
            if (lesson.TeacherLessons == null || !lesson.TeacherLessons.Any(tl => tl.TeacherId == Guid.Parse(teacherId)))
            {
                return Result<CourseCreateAndEditDto>.Failure($"Guru tidak memiliki pelajaran ini");
            }

            /** Langkah 3: Membuat entity Course dari DTO **/
            var course = await _context.Courses.FindAsync(request.CourseId);

            if (course == null)
            {
                return Result<CourseCreateAndEditDto>.Failure("Materi tidak ditemukan");
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
                return Result<CourseCreateAndEditDto>.Failure("Gagal untuk mengedit materi");
            }

            /** Langkah 6: Mengembalikan hasil **/
            var courseDto = _mapper.Map<CourseCreateAndEditDto>(course);
            courseDto.FileData = request.CourseCreateAndEditDto.FileData; // Menambahkan FileData ke DTO
            courseDto.LessonName = lesson?.LessonName; // Menambahkan LessonName ke DTO

            return Result<CourseCreateAndEditDto>.Success(courseDto);
        }
    }
}