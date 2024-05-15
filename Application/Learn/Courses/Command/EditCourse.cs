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
    public class Command : IRequest<Result<CourseEditDto>>
    {
        public Guid CourseId { get; set; }
        public CourseEditDto CourseEditDto { get; set; }
    }

    public class Handler : IRequestHandler<Command, Result<CourseEditDto>>
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

        public async Task<Result<CourseEditDto>> Handle(Command request, CancellationToken cancellationToken)
        {
            /** Langkah 1: Memeriksa teacherId **/
            var teacherId = _userAccessor.GetTeacherIdFromToken();
            if (teacherId == null)
            {
                return Result<CourseEditDto>.Failure("TeacherId tidak ditemukan ditoken");
            }

            /** Langkah 2: Memeriksa LessonName dan mendapatkan LessonId **/
            var lesson = await _context.Lessons
                .Include(tl => tl.TeacherLessons)
                .FirstOrDefaultAsync(x => x.LessonName == request.CourseEditDto.LessonName);

            // Memeriksa apakah lesson yang dimasukkan ada didatabase
            if (lesson == null)
            {
                return Result<CourseEditDto>.Failure($"Mapel dengan nama mapel {request.CourseEditDto.LessonName} tidak ditemukan");
            }

            // Memeriksa apakah teacher memiliki keterkaitan dengan lesson yang dimasukkan
            if (lesson.TeacherLessons == null || !lesson.TeacherLessons.Any(tl => tl.TeacherId == Guid.Parse(teacherId)))
            {
                return Result<CourseEditDto>.Failure($"Guru tidak memiliki pelajaran ini");
            }

            /** Langkah 3: Membuat entity Course dari DTO **/
            var course = await _context.Courses.FindAsync(request.CourseId);

            if (course == null)
            {
                return Result<CourseEditDto>.Failure("Materi tidak ditemukan");
            }

            /** Langkah 4: Menyimpan file jika ada **/
            //string filePath = null;
            //if (request.CourseEditDto.FileData != null)
            //{
            //    string fileExtension = Path.GetExtension(request.CourseEditDto.FileData.FileName);
            //    if (!string.Equals(fileExtension, ".pdf", StringComparison.OrdinalIgnoreCase))
            //    {
            //        return Result<CourseEditDto>.Failure("Hanya file PDF yang diperbolehkan");
            //    }

            //    string relativeFolderPath = "Upload/FileCourse";
            //    filePath = await _fileService.SaveFileAsync(request.CourseEditDto.FileData, relativeFolderPath, request.CourseEditDto.CourseName, course.CreatedAt);
            //}

            // Membuat Mapper untuk edit course
            _mapper.Map(request.CourseEditDto, course, opts =>
            {
                opts.Items["Lesson"] = lesson; // Menambahkan Lesson ke context untuk digunakan dalam AfterMap
            });

            /** Langkah 5: Menyimpan Course ke database **/
            var result = await _context.SaveChangesAsync() > 0;
            if (!result)
            {
                return Result<CourseEditDto>.Failure("Gagal untuk mengedit materi");
            }

            /** Langkah 6: Mengembalikan hasil **/
            var courseDto = _mapper.Map<CourseEditDto>(course);
            courseDto.LessonName = lesson?.LessonName; // Menambahkan LessonName ke DTO

            return Result<CourseEditDto>.Success(courseDto);
        }
    }
}

public class CommandValidatorEditDto : AbstractValidator<CourseEditDto>
{
    public CommandValidatorEditDto()
    {
        RuleFor(x => x.CourseName).NotEmpty().WithMessage("Nama materi tidak boleh kosong");
        RuleFor(x => x.Description).NotEmpty().WithMessage("Deskripsi tidak boleh kosong");
        RuleFor(x => x.LessonName).NotEmpty().WithMessage("Nama mapel tidak boleh kosong");
    }
}