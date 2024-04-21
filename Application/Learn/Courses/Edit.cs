using Application.Core;
using Application.Interface;
using AutoMapper;
using Domain.Many_to_Many;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Learn.Courses
{
    public class EditCourse
{
    public class Command : IRequest<Result<CourseDto>>
    {
        public Guid CourseId { get; set; }
        public CourseDto CourseDto { get; set; }
    }

    public class CommandValidator : AbstractValidator<Command>
    {
        public CommandValidator()
        {
            RuleFor(x => x.CourseId).NotEmpty();
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
                var existingCourse = await _context.Courses
                    .Include(c => c.CourseClassRooms)
                    .FirstOrDefaultAsync(c => c.Id == request.CourseId, cancellationToken);

                if (existingCourse == null)
                    return Result<CourseDto>.Failure($"Course with id {request.CourseId} not found.");

                // Memeriksa apakah teacherId tidak kosong
                if (request.CourseDto.TeacherId == Guid.Empty)
                    return Result<CourseDto>.Failure($"TeacherId cannot be empty.");

                // Membaca data file jika CourseDto.FileData tidak null
                byte[]? fileData = existingCourse.FileData;
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
                    .FirstOrDefaultAsync(x => x.LessonName == request.CourseDto.LessonName, cancellationToken);

                if (lesson == null)
                    return Result<CourseDto>.Failure($"Lesson with LessonName {request.CourseDto.LessonName} not found.");

                // Memeriksa apakah teacher memiliki keterkaitan dengan lesson yang dimasukkan
                var teacherId = Guid.Parse(_userAccessor.GetTeacherIdFromToken());
                if (lesson.TeacherLessons.All(tl => tl.TeacherId != teacherId))
                    return Result<CourseDto>.Failure($"Teacher does not have this lesson.");

                // Memeriksa apakah teacherId memiliki keterkaitan dengan setiap uniqueNumberOfClassRoom yang dimasukkan
                foreach (var uniqueNumberOfClassRoom in request.CourseDto.UniqueNumberOfClassRooms)
                {
                    var classRoom = await _context.ClassRooms.FirstOrDefaultAsync(x => x.UniqueNumberOfClassRoom == uniqueNumberOfClassRoom, cancellationToken);
                    if (classRoom == null)
                        return Result<CourseDto>.Failure($"ClassRoom with UniqueNumberOfClassRoom {uniqueNumberOfClassRoom} not found.");

                    // Memeriksa keterkaitan teacherId dengan classroom
                    if (!_context.TeacherClassRooms.Any(tc => tc.TeacherId == teacherId && tc.ClassRoomId == classRoom.Id))
                        return Result<CourseDto>.Failure($"Teacher is not assigned to Classroom {uniqueNumberOfClassRoom}.");
                }

                // Update data course yang ada dengan data baru
                existingCourse.CourseName = request.CourseDto.CourseName;
                existingCourse.Description = request.CourseDto.Description;
                existingCourse.FileData = fileData;
                existingCourse.LinkCourse = request.CourseDto.LinkCourse;
                existingCourse.LessonId = lesson.Id; // Mengisi LessonId dengan Id Lesson yang sesuai

                // Menghapus kelas yang terkait sebelum menambahkan yang baru
                _context.CourseClassRooms.RemoveRange(existingCourse.CourseClassRooms);

                // Menambahkan kelas yang baru
                var addedClassRooms = new List<string>();
                foreach (var uniqueNumberOfClassRoom in request.CourseDto.UniqueNumberOfClassRooms)
                {
                    // Temukan ClassRoom yang sesuai berdasarkan UniqueNumberOfClassRoom
                    var classRoom = await _context.ClassRooms.FirstOrDefaultAsync(x => x.UniqueNumberOfClassRoom == uniqueNumberOfClassRoom, cancellationToken);
                    if (classRoom == null)
                        return Result<CourseDto>.Failure($"ClassRoom with UniqueNumberOfClassRoom {uniqueNumberOfClassRoom} not found.");

                    // Buat entri baru di CourseClassRoom
                    existingCourse.CourseClassRooms.Add(new CourseClassRoom
                    {
                        CourseId = existingCourse.Id,
                        ClassRoomId = classRoom.Id
                    });

                    addedClassRooms.Add(classRoom.UniqueNumberOfClassRoom);
                }

                await _context.SaveChangesAsync(cancellationToken);

                // Mengembalikan CourseDto yang berhasil diubah bersama dengan TeacherId dan UniqueNumberOfClassRooms yang berhasil ditambahkan
                var courseDto = _mapper.Map<CourseDto>(existingCourse);
                courseDto.LessonName = lesson.LessonName;
                courseDto.UniqueNumberOfClassRooms = addedClassRooms;
                courseDto.TeacherId = teacherId; // Menambahkan TeacherId ke dalam CourseDto
                return Result<CourseDto>.Success(courseDto);

            }
            catch (Exception ex)
            {
                // Menangani kesalahan dan mengembalikan pesan kesalahan yang sesuai
                return Result<CourseDto>.Failure($"Failed to update course: {ex.Message}");
            }
        }
    }
}
}
