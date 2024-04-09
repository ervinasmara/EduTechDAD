using Application.Core;
using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Learn.Agenda
{
    public class List
    {
        public class Query : IRequest<Result<List<ScheduleGetDto>>>
        {
            // Tidak diperlukan parameter tambahan
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
                var schedules = await _context.Schedules
                    .Include(s => s.Lesson)
                    .Include(s => s.ClassRoom)
                    .ToListAsync(cancellationToken);

                var scheduleDtos = _mapper.Map<List<ScheduleGetDto>>(schedules);

                // Map ID dari Schedule ke ScheduleGetDto
                for (int i = 0; i < schedules.Count; i++)
                {
                    scheduleDtos[i].Id = schedules[i].Id;
                }

                return Result<List<ScheduleGetDto>>.Success(scheduleDtos);
            }
        }
    }
}