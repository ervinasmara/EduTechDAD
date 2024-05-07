using Application.Core;
using Application.Interface;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Learn.Courses.Command;
public class DeactivateCourse
{
    public class Command : IRequest<Result<object>>
    {
        public Guid CourseId { get; set; }
    }

    public class EditCourseStatusHandler : IRequestHandler<Command, Result<object>>
    {
        private readonly DataContext _context;
        private readonly IUserAccessor _userAccessor;

        public EditCourseStatusHandler(DataContext context, IUserAccessor userAccessor)
        {
            _context = context;
            _userAccessor = userAccessor;
        }

        public async Task<Result<object>> Handle(Command request, CancellationToken cancellationToken)
        {
            /** Langkah 1: Dapatkan TeacherId dari token **/
            var teacherIdString = _userAccessor.GetTeacherIdFromToken();

            /** Langkah 2: Periksa jika TeacherId ada di token **/
            if (string.IsNullOrEmpty(teacherIdString))
            {
                return Result<object>.Failure("TeacherId tidak ditemukan ditoken");
            }

            if (!Guid.TryParse(teacherIdString, out var teacherId))
            {
                return Result<object>.Failure("TeacherId tidak valid.");
            }

            /** Langkah 3: Ubah status Course menjadi 0 **/
            var course = await _context.Courses
                .Include(l => l.Lesson)
                    .ThenInclude(l => l.TeacherLessons)
                .FirstOrDefaultAsync(c => c.Id == request.CourseId);

            // Periksa apakah ada course yang ditemukan
            if (course == null)
            {
                return Result<object>.Failure("Materi tidak ditemukan");
            }

            // Memeriksa apakah teacher memiliki keterkaitan dengan course yang dimasukkan
            if (course.Lesson.TeacherLessons == null || !course.Lesson.TeacherLessons.Any(tl => tl.TeacherId == teacherId))
            {
                return Result<object>.Failure($"Guru tidak memiliki materi ini.");
            }

            /** Langkah 4: Mengubah status course menjadi 0 (nonaktif) **/
            course.Status = 0;

            /** Langkah 5: Simpan perubahan status ke database **/
            await _context.SaveChangesAsync(cancellationToken);

            /** Langkah 6: Mengembalikan hasil berhasil dengan pesan **/
            return Result<object>.Success(new { Message = "Status materi berhasil diperbarui" });
        }
    }
}