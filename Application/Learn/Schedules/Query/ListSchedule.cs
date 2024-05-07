using Application.Core;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Learn.Schedules.Query;
public class ListSchedule
{
    public class Query : IRequest<Result<List<ScheduleGetDto>>>
    {
    }

    public class Handler : IRequestHandler<Query, Result<List<ScheduleGetDto>>>
    {
        private readonly DataContext _context;
        private readonly IMapper _mapper;

        public Handler(DataContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<Result<List<ScheduleGetDto>>> Handle(Query request, CancellationToken cancellationToken)
        {
            try
            {
                /** Langkah 1: Mengambil Jadwal dan Memetakkannya ke ScheduleGetDto **/
                var schedule = await _context.Schedules
                    .ProjectTo<ScheduleGetDto>(_mapper.ConfigurationProvider)
                    .ToListAsync(cancellationToken);

                /** Langkah 2: Mengembalikan Hasil dalam Bentuk Success Result **/
                return Result<List<ScheduleGetDto>>.Success(schedule);
            }
            catch (Exception ex)
            {
                /** Langkah 3: Menangani Kesalahan Jika Terjadi **/
                return Result<List<ScheduleGetDto>>.Failure($"Gagal mengambil jadwal: {ex.Message}");
            }
        }
    }
}