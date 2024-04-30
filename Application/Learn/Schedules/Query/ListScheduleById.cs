using Application.Core;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Learn.Schedules.Query
{
    public class ListScheduleById
    {
        public class Query : IRequest<Result<ScheduleGetDto>>
        {
            public Guid ScheduleId { get; set; }
        }

        public class Handler : IRequestHandler<Query, Result<ScheduleGetDto>>
        {
            private readonly DataContext _context;
            private readonly IMapper _mapper;

            public Handler(DataContext context, IMapper mapper)
            {
                _context = context;
                _mapper = mapper;
            }

            public async Task<Result<ScheduleGetDto>> Handle(Query request, CancellationToken cancellationToken)
            {
                try
                {
                    var schedule = await _context.Schedules
                    .ProjectTo<ScheduleGetDto>(_mapper.ConfigurationProvider)
                    .FirstOrDefaultAsync(s => s.Id == request.ScheduleId, cancellationToken);

                    if (schedule == null)
                    {
                        return Result<ScheduleGetDto>.Failure("Schedule not found");
                    }

                    return Result<ScheduleGetDto>.Success(schedule);
                }
                catch (Exception ex)
                {
                    return Result<ScheduleGetDto>.Failure($"Failed to retrieve schedule: {ex.Message}");
                }
            }
        }
    }
}