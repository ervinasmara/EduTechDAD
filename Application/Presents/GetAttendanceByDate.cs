using Application.Core;
using Domain.Present;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Presents
{
    public class GetAttendanceByDate
    {
        public class Query : IRequest<Result<List<Attendance>>>
        {
            public int Year { get; set; }
            public int Month { get; set; }
            public int Day { get; set; }
        }

        public class Handler : IRequestHandler<Query, Result<List<Attendance>>> // Mengubah Result<Attendance> menjadi Result<List<Attendance>>
        {
            private readonly DataContext _context;

            public Handler(DataContext context)
            {
                _context = context;
            }

            public async Task<Result<List<Attendance>>> Handle(Query request, CancellationToken cancellationToken) // Mengubah Result<Attendance> menjadi Result<List<Attendance>>
            {
                var date = new DateOnly(request.Year, request.Month, request.Day);
                var attendances = await _context.Attendances.Where(a => a.Date == date).ToListAsync(cancellationToken); // Menggunakan Where dan ToListAsync untuk mendapatkan semua data yang cocok

                if (attendances == null || attendances.Count == 0) // Ubah pengecekan jika tidak ada data yang ditemukan
                    return Result<List<Attendance>>.Failure("No attendance found for the specified date.");

                return Result<List<Attendance>>.Success(attendances);
            }
        }
    }
}