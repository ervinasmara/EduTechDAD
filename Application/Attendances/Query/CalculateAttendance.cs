using Application.Core;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Attendances.Query;
public class CalculateAttendance
{
    public class AttendanceQuery : IRequest<Result<AttendanceSummaryDto>>
    {
        public string? UniqueNumberOfClassRoom { get; set; }
        public string Year { get; set; }
        public string Month { get; set; }
    }

    public class AttendanceQueryHandler : IRequestHandler<AttendanceQuery, Result<AttendanceSummaryDto>>
    {
        private readonly DataContext _context;

        public AttendanceQueryHandler(DataContext context)
        {
            _context = context;
        }

        public async Task<Result<AttendanceSummaryDto>> Handle(AttendanceQuery request, CancellationToken cancellationToken)
        {
            /** Langkah 1: Memeriksa ketersediaan parameter tahun dan bulan **/
            if (string.IsNullOrEmpty(request.Year) || string.IsNullOrEmpty(request.Month))
            {
                return Result<AttendanceSummaryDto>.Failure("Parameter Tahun dan Bulan diperlukan.");
            }

            /** Langkah 2: Menentukan rentang tanggal berdasarkan tahun dan bulan **/
            var startDate = new DateOnly(int.Parse(request.Year), int.Parse(request.Month), 1);
            var endDate = startDate.AddMonths(1).AddDays(-1);

            /** Langkah 3: Mendapatkan daftar kehadiran dalam rentang tanggal yang ditentukan **/
            var attendances = _context.Attendances
                .Where(a => a.Date >= startDate && a.Date <= endDate)
                .AsQueryable();

            var uniqueNumber = await _context.ClassRooms
                .Where(a => a.UniqueNumberOfClassRoom == request.UniqueNumberOfClassRoom)
                .Select(a => a.Id)
                .FirstOrDefaultAsync(cancellationToken);

            /** Langkah 4: Filter berdasarkan ID ruang kelas jika disediakan **/
            if (!string.IsNullOrEmpty(request.UniqueNumberOfClassRoom) && uniqueNumber != Guid.Empty)
            {
                attendances = attendances.Where(a => a.Student.ClassRoomId == uniqueNumber);
            }

            /** Langkah 5: Menghitung jumlah kehadiran berdasarkan status **/
            var summary = new AttendanceSummaryDto
            {
                PresentCount = await attendances.CountAsync(a => a.Status == 1, cancellationToken),
                ExcusedCount = await attendances.CountAsync(a => a.Status == 2, cancellationToken),
                AbsentCount = await attendances.CountAsync(a => a.Status == 3, cancellationToken)
            };

            return Result<AttendanceSummaryDto>.Success(summary);
        }
    }
}