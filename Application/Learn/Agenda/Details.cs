using Application.Core;
using AutoMapper;
using Domain.Learn.Agenda;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Learn.Agenda
{
    public class Details
    {
        public class Query : IRequest<Result<List<ScheduleGetDto>>>
        {
            public Guid ClassRoomId { get; set; }

            public Query(Guid classRoomId)
            {
                ClassRoomId = classRoomId;
            }
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
                // Cek apakah ClassRoom dengan ClassRoomId yang diberikan ada di database
                var classRoom = await _context.ClassRooms.FindAsync(request.ClassRoomId);
                if (classRoom == null)
                    return Result<List<ScheduleGetDto>>.Failure("ClassRoom not found with the provided ClassRoomId");

                var schedules = await _context.Schedules
                    .Where(s => s.ClassRoomId == request.ClassRoomId)
                    .Include(s => s.Lesson)
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