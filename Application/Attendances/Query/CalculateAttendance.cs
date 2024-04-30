using Application.Core;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Attendances.Query
{
    public class CalculateAttendance
    {
        public class AttendanceQuery : IRequest<Result<AttendanceSummaryDto>>
        {
            public Guid? ClassRoomId { get; set; }
        }

        public class Handler : IRequestHandler<AttendanceQuery, Result<AttendanceSummaryDto>>
        {
            private readonly DataContext _context;

            public Handler(DataContext context)
            {
                _context = context;
            }

            public async Task<Result<AttendanceSummaryDto>> Handle(AttendanceQuery query, CancellationToken cancellationToken)
            {
                var attendanceQuery = _context.Attendances.AsQueryable();

                if (query.ClassRoomId.HasValue)
                {
                    attendanceQuery = attendanceQuery.Where(a => a.Student.ClassRoomId == query.ClassRoomId.Value);
                }

                // Menggunakan ProjectTo untuk memproyeksikan hasil langsung ke DTO
                var summary = await attendanceQuery
                    .GroupBy(a => 1) // Grup berdasarkan nilai konstan untuk mengumpulkan semua data
                    .Select(g => new AttendanceSummaryDto
                    {
                        PresentCount = g.Count(a => a.Status == 1),
                        ExcusedCount = g.Count(a => a.Status == 2),
                        AbsentCount = g.Count(a => a.Status == 3)
                    })
                    .FirstOrDefaultAsync(cancellationToken) ?? new AttendanceSummaryDto();

                return Result<AttendanceSummaryDto>.Success(summary);
            }
        }
    }
}
