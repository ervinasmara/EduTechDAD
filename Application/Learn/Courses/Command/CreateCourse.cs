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
    public class Command : IRequest<Result<CourseCreateDto>>
    {
        public CourseCreateDto CourseCreateDto { get; set; }
    }

    public class Handler : IRequestHandler<Command, Result<CourseCreateDto>>
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

        public async Task<Result<CourseCreateDto>> Handle(Command request, CancellationToken cancellationToken)
        {
            /** Langkah 1: Memeriksa teacherId **/
            var teacherId = _userAccessor.GetTeacherIdFromToken();
            if (teacherId == null)
            {
                return Result<CourseCreateDto>.Failure("TeacherId tidak ditemukan ditoken");
            }

            /** Langkah 2: Memeriksa LessonName dan mendapatkan LessonId **/
            var lesson = await _context.Lessons
                .Include(tl => tl.TeacherLessons)
                .FirstOrDefaultAsync(x => x.LessonName == request.CourseCreateDto.LessonName);
            if (lesson == null)
            {
                return Result<CourseCreateDto>.Failure($"Mapel dengan nama mapel {request.CourseCreateDto.LessonName} tidak ditemukan");
            }

            // Memeriksa apakah teacher memiliki keterkaitan dengan lesson yang dimasukkan
            if (lesson.TeacherLessons == null || !lesson.TeacherLessons.Any(tl => tl.TeacherId == Guid.Parse(teacherId)))
            {
                return Result<CourseCreateDto>.Failure($"Guru tidak memiliki pelajaran ini");
            }

            /** Langkah 3: Membuat entity Course dari DTO **/
            var course = _mapper.Map<Course>(request.CourseCreateDto, opts =>
            {
                opts.Items["Lesson"] = lesson; // Menambahkan Lesson ke context untuk digunakan dalam AfterMap
            });

            /** Langkah 4: Menyimpan file jika ada **/
            string filePath = null;
            if (request.CourseCreateDto.FileData != null)
            {
                string fileExtension = Path.GetExtension(request.CourseCreateDto.FileData.FileName);
                if (!string.Equals(fileExtension, ".pdf", StringComparison.OrdinalIgnoreCase))
                {
                    return Result<CourseCreateDto>.Failure("Hanya file PDF yang diperbolehkan");
                }

                string relativeFolderPath = "/app/uploads/course";
                filePath = await _fileService.SaveFileAsync(request.CourseCreateDto.FileData, relativeFolderPath, request.CourseCreateDto.CourseName, course.CreatedAt);
            }

            // Setelah menyimpan file, set FilePath pada course
            course.FilePath = filePath;

            /** Langkah 5: Menyimpan Course ke database **/
            _context.Courses.Add(course);
            var result = await _context.SaveChangesAsync() > 0;
            if (!result)
            {
                return Result<CourseCreateDto>.Failure("Gagal untuk membuat course.");
            }

            /** Langkah 6: Mengembalikan hasil **/
            var courseDto = _mapper.Map<CourseCreateDto>(course);
            courseDto.FileData = request.CourseCreateDto.FileData; // Menambahkan FileData ke DTO
            courseDto.LessonName = lesson?.LessonName; // Menambahkan LessonName ke DTO

            return Result<CourseCreateDto>.Success(courseDto);
        }
    }
}

public class CommandValidatorCreateDto : AbstractValidator<CourseCreateDto>
{
    public CommandValidatorCreateDto()
    {
        RuleFor(x => x.CourseName).NotEmpty().WithMessage("Nama materi tidak boleh kosong");
        RuleFor(x => x.Description).NotEmpty().WithMessage("Deskripsi tidak boleh kosong");
        RuleFor(x => x.LessonName).NotEmpty().WithMessage("Nama mapel tidak boleh kosong");

        // Validasi untuk memastikan bahwa setidaknya satu dari LinkCourse diisi
        RuleFor(x => x.LinkCourse)
            .NotEmpty()
            .When(x => x.FileData == null) // Hanya memeriksa LinkCourse jika FileData kosong
            .WithMessage("Link materi harus disediakan jika File tidak disediakan.");
    }
}
