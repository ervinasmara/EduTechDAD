using Application.Core;
using Application.Interface;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Learn.Schedules.Query;
public class ListSchedulesByClassRoomId
{
    public class Query : IRequest<Result<List<ScheduleGetDto>>>
    {
        // Tidak memerlukan properti karena hanya mengambil informasi dari token
    }

    public class Handler : IRequestHandler<Query, Result<List<ScheduleGetDto>>>
    {
        private readonly DataContext _context;
        private readonly IMapper _mapper;
        private readonly IUserAccessor _userAccessor;

        public Handler(DataContext context, IUserAccessor userAccessor, IMapper mapper)
        {
            _context = context;
            _userAccessor = userAccessor;
            _mapper = mapper;
        }

        public async Task<Result<List<ScheduleGetDto>>> Handle(Query request, CancellationToken cancellationToken)
        {
            try
            {
                /** Langkah 1: Mendapatkan ID Ruang Kelas dari Token **/
                var classRoomId = _userAccessor.GetClassRoomIdFromToken();

                /** Langkah 2: Memeriksa Ketersediaan ID Ruang Kelas **/
                if (string.IsNullOrEmpty(classRoomId))
                    return Result<List<ScheduleGetDto>>.Failure("Classroom ID not found in token");

                /** Langkah 3: Mengambil Jadwal **/
                var schedule = await _context.Schedules
                    .ProjectTo<ScheduleGetDto>(_mapper.ConfigurationProvider)
                    .ToListAsync(cancellationToken);

                /** Langkah 4: Memeriksa Ketersediaan Jadwal untuk Ruang Kelas Tertentu **/
                if (!schedule.Any())
                    return Result<List<ScheduleGetDto>>.Failure("Schedules not found for this classroom.");

                /** Langkah 5: Mengembalikan Hasil dalam Bentuk Success Result **/
                return Result<List<ScheduleGetDto>>.Success(schedule);
            }
            catch (Exception ex)
            {
                /** Langkah 6: Menangani Kesalahan Jika Terjadi **/
                return Result<List<ScheduleGetDto>>.Failure($"Failed to retrieve schedules: {ex.Message}");
            }
        }
    }
}