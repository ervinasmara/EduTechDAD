using Application.Core;
using AutoMapper;
using Domain.Many_to_Many;
using Domain.Learn.Courses;
using FluentValidation;
using MediatR;
using Persistence;
using Microsoft.EntityFrameworkCore;
using Application.Interface;

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
            private readonly IUserAccessor _userAccessor;

            public Handler(DataContext context, IMapper mapper, IUserAccessor userAccessor)
            {
                _context = context;
                _mapper = mapper;
                _userAccessor = userAccessor;
            }

            public async Task<Result<CourseDto>> Handle(Command request, CancellationToken cancellationToken)
            {
                try
                {
                    // Memeriksa apakah teacherId tidak kosong
                    if (request.CourseDto.TeacherId == Guid.Empty)
                        return Result<CourseDto>.Failure($"TeacherId cannot be empty.");

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

                    // Menemukan Lesson yang sesuai berdasarkan LessonName
                    var lesson = await _context.Lessons
                        .Include(l => l.TeacherLessons)
                        .ThenInclude(tl => tl.Teacher)
                        .FirstOrDefaultAsync(x => x.LessonName == request.CourseDto.LessonName);

                    if (lesson == null)
                        return Result<CourseDto>.Failure($"Lesson with LessonName {request.CourseDto.LessonName} not found.");

                    // Memeriksa apakah teacher memiliki keterkaitan dengan lesson yang dimasukkan
                    var teacherId = Guid.Parse(_userAccessor.GetTeacherIdFromToken());
                    if (lesson.TeacherLessons.All(tl => tl.TeacherId != teacherId))
                        return Result<CourseDto>.Failure($"Teacher does not have this lesson.");

                    // Memeriksa apakah teacherId memiliki keterkaitan dengan setiap uniqueNumberOfClassRoom yang dimasukkan
                    foreach (var uniqueNumberOfClassRoom in request.CourseDto.UniqueNumberOfClassRooms)
                    {
                        var classRoom = await _context.ClassRooms.FirstOrDefaultAsync(x => x.UniqueNumberOfClassRoom == uniqueNumberOfClassRoom);
                        if (classRoom == null)
                            return Result<CourseDto>.Failure($"ClassRoom with UniqueNumberOfClassRoom {uniqueNumberOfClassRoom} not found.");

                        // Memeriksa keterkaitan teacherId dengan classroom
                        if (!_context.TeacherClassRooms.Any(tc => tc.TeacherId == teacherId && tc.ClassRoomId == classRoom.Id))
                            return Result<CourseDto>.Failure($"Teacher is not assigned to Classroom {uniqueNumberOfClassRoom}.");
                    }

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

                    // Simpan TeacherId dan CourseId dalam entitas TeacherCourse
                    var teacherCourse = new TeacherCourse
                    {
                        TeacherId = teacherId,
                        CourseId = course.Id
                    };
                    _context.TeacherCourses.Add(teacherCourse);
                    await _context.SaveChangesAsync(cancellationToken);

                    // Loop melalui koleksi UniqueNumberOfClassRooms
                    var addedClassRooms = new List<string>();
                    foreach (var uniqueNumberOfClassRoom in request.CourseDto.UniqueNumberOfClassRooms)
                    {
                        // Temukan ClassRoom yang sesuai berdasarkan UniqueNumberOfClassRoom
                        var classRoom = await _context.ClassRooms.FirstOrDefaultAsync(x => x.UniqueNumberOfClassRoom == uniqueNumberOfClassRoom);
                        if (classRoom == null)
                            return Result<CourseDto>.Failure($"ClassRoom with UniqueNumberOfClassRoom {uniqueNumberOfClassRoom} not found.");

                        // Buat entri baru di CourseClassRoom
                        _context.CourseClassRooms.Add(new CourseClassRoom
                        {
                            CourseId = course.Id,
                            ClassRoomId = classRoom.Id
                        });

                        addedClassRooms.Add(classRoom.UniqueNumberOfClassRoom);
                    }

                    await _context.SaveChangesAsync(cancellationToken);

                    // Mengembalikan CourseDto yang baru dibuat bersama dengan TeacherId dan UniqueNumberOfClassRooms yang berhasil ditambahkan
                    var courseDto = _mapper.Map<CourseDto>(course);
                    courseDto.LessonName = lesson.LessonName;
                    courseDto.UniqueNumberOfClassRooms = addedClassRooms;
                    courseDto.TeacherId = teacherId; // Menambahkan TeacherId ke dalam CourseDto
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