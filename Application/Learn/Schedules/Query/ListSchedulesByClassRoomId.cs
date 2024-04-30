using Application.Core;
using Application.Interface;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Learn.Schedules.Query
{
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
                    var classRoomId = _userAccessor.GetClassRoomIdFromToken();

                    if (string.IsNullOrEmpty(classRoomId))
                        return Result<List<ScheduleGetDto>>.Failure("Classroom ID not found in token");

                    var schedule = await _context.Schedules
                    .ProjectTo<ScheduleGetDto>(_mapper.ConfigurationProvider)
                    .ToListAsync(cancellationToken);

                    if (!schedule.Any())
                        return Result<List<ScheduleGetDto>>.Failure("Schedules not found for this classroom.");

                    return Result<List<ScheduleGetDto>>.Success(schedule);
                }
                catch (Exception ex)
                {
                    return Result<List<ScheduleGetDto>>.Failure($"Failed to retrieve schedules: {ex.Message}");
                }
            }
        }
    }
}
