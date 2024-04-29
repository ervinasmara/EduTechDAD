using Application.Core;
using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Attendances.Query
{
    public class GetAllByStudentId
    {
        public class Query : IRequest<Result<List<AttendanceGetAllDto>>>
        {
            public Guid StudentId { get; set; }
        }

        public class Handler : IRequestHandler<Query, Result<List<AttendanceGetAllDto>>>
        {
            private readonly DataContext _context;
            private readonly IMapper _mapper;

            public Handler(DataContext context, IMapper mapper)
            {
                _context = context;
                _mapper = mapper;
            }

            public async Task<Result<List<AttendanceGetAllDto>>> Handle(Query request, CancellationToken cancellationToken)
            {
                var attendances = await _context.Attendances
                    .Where(a => a.StudentId == request.StudentId)
                    .ToListAsync(cancellationToken);

                var attendanceDtos = _mapper.Map<List<AttendanceGetAllDto>>(attendances);

                return Result<List<AttendanceGetAllDto>>.Success(attendanceDtos);
            }
        }
    }
}