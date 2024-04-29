using Application.Core;
using Application.Interface;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Learn.Courses.Command
{
    public class DeactivateCourse
    {
        public class Command : IRequest<Result<object>>
        {
            public Guid CourseId { get; set; }
        }

        public class Validator : AbstractValidator<Command>
        {
            public Validator()
            {
                RuleFor(x => x.CourseId).NotEmpty();
            }
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
                    return Result<object>.Failure("TeacherId not found in token");
                }

                if (!Guid.TryParse(teacherIdString, out var teacherId))
                {
                    return Result<object>.Failure("TeacherId not valid.");
                }

                /** Langkah 3: Ubah status Course menjadi 0 **/
                var course = await _context.Courses
                    .Include(l => l.Lesson)
                        .ThenInclude(l => l.TeacherLessons)
                    .FirstOrDefaultAsync(c => c.Id == request.CourseId);

                // Periksa apakah ada course yang ditemukan
                if (course == null)
                {
                    return Result<object>.Failure("Course not found");
                }

                // Memeriksa apakah teacher memiliki keterkaitan dengan course yang dimasukkan
                if (course.Lesson.TeacherLessons == null || !course.Lesson.TeacherLessons.Any(tl => tl.TeacherId == teacherId))
                {
                    return Result<object>.Failure($"Teacher does not have this course.");
                }

                /** Langkah 4: Mengubah status course menjadi 0 (nonaktif) **/
                course.Status = 0;

                /** Langkah 5: Simpan perubahan status ke database **/
                await _context.SaveChangesAsync(cancellationToken);

                /** Langkah 6: Mengembalikan hasil berhasil dengan pesan **/
                return Result<object>.Success(new { Message = "Course status updated successfully" });
            }
        }
    }
}
