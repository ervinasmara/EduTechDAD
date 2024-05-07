using Application.Core;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Attendances.Query;
public class GetAllByStudentId
{
    public class Query : IRequest<Result<List<AttendanceGetByStudentIdDto>>>
    {
        public Guid StudentId { get; set; }
    }

    public class Handler : IRequestHandler<Query, Result<List<AttendanceGetByStudentIdDto>>>
    {
        private readonly DataContext _context;
        private readonly IMapper _mapper;

        public Handler(DataContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<Result<List<AttendanceGetByStudentIdDto>>> Handle(Query request, CancellationToken cancellationToken)
        {
            var attendances = await _context.Attendances
                .Where(a => a.StudentId == request.StudentId)
                .ProjectTo<AttendanceGetByStudentIdDto>(_mapper.ConfigurationProvider)
                .OrderByDescending(a => a.Date)
                .ToListAsync(cancellationToken);

            return Result<List<AttendanceGetByStudentIdDto>>.Success(attendances);
        }
    }
}