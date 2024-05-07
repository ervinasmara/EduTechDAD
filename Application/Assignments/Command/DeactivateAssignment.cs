using Application.Core;
using Application.Interface;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Assignments.Command;
public class DeactivateAssignment
{
    public class Command : IRequest<Result<object>>
    {
        public Guid AssignmentId { get; set; }
    }

    public class EditAssignmentStatusHandler : IRequestHandler<Command, Result<object>>
    {
        private readonly DataContext _context;
        private readonly IUserAccessor _userAccessor;

        public EditAssignmentStatusHandler(DataContext context, IUserAccessor userAccessor)
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

            /** Langkah 3: Ubah status Assignment menjadi 0 **/
            var assignment = await _context.Assignments
                .Include(l => l.Course)
                    .ThenInclude(l => l.Lesson)
                        .ThenInclude(l => l.TeacherLessons)
                .FirstOrDefaultAsync(c => c.Id == request.AssignmentId);

            // Periksa apakah ada assignment yang ditemukan
            if (assignment == null)
            {
                return Result<object>.Failure("Tugas tidak ditemukan");
            }

            // Memeriksa apakah teacher memiliki keterkaitan dengan assignment yang dimasukkan
            if (assignment.Course.Lesson.TeacherLessons == null || !assignment.Course.Lesson.TeacherLessons.Any(tl => tl.TeacherId == teacherId))
            {
                return Result<object>.Failure($"Guru tidak memiliki tugas ini");
            }

            /** Langkah 4: Mengubah status assignment menjadi 0 (nonaktif) **/
            assignment.Status = 0;

            /** Langkah 5: Simpan perubahan status ke database **/
            await _context.SaveChangesAsync(cancellationToken);

            /** Langkah 6: Mengembalikan hasil berhasil dengan pesan **/
            return Result<object>.Success(new { Message = "Status tugas berhasil diupdate" });
        }
    }
}