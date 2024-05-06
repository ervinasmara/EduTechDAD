using Application.Core;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Assignments.Query;
public class DetailsAssignment
{
    public class Query : IRequest<Result<AssignmentGetByIdDto>>
    {
        public Guid AssignmentId { get; set; }
    }

    public class Handler : IRequestHandler<Query, Result<AssignmentGetByIdDto>>
    {
        private readonly DataContext _context;
        private readonly IMapper _mapper;

        public Handler(DataContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<Result<AssignmentGetByIdDto>> Handle(Query request, CancellationToken cancellationToken)
        {
            // Gunakan ProjectTo untuk memproyeksikan entitas ke DTO
            var assignmentDto = await _context.Assignments
                .Where(a => a.Id == request.AssignmentId && a.Status != 0)
                .ProjectTo<AssignmentGetByIdDto>(_mapper.ConfigurationProvider)
                .FirstOrDefaultAsync(cancellationToken);

            // Periksa apakah assignment ditemukan
            if (assignmentDto == null)
            {
                return Result<AssignmentGetByIdDto>.Failure("Assignment not found.");
            }

            return Result<AssignmentGetByIdDto>.Success(assignmentDto);
        }
    }
}