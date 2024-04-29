using Application.Core;
using Application.Interface;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Assignments.Query
{
    public class DetailsAssignment
    {
        public class Query : IRequest<Result<AssignmentGetAllAndByIdDto>>
        {
            public Guid AssignmentId { get; set; }
        }

        public class Handler : IRequestHandler<Query, Result<AssignmentGetAllAndByIdDto>>
        {
            private readonly DataContext _context;
            private readonly IUserAccessor _userAccessor;
            private readonly IMapper _mapper;

            public Handler(DataContext context, IUserAccessor userAccessor, IMapper mapper)
            {
                _context = context;
                _userAccessor = userAccessor;
                _mapper = mapper;
            }

            public async Task<Result<AssignmentGetAllAndByIdDto>> Handle(Query request, CancellationToken cancellationToken)
            {
                // Gunakan ProjectTo untuk memproyeksikan entitas ke DTO
                var assignmentDto = await _context.Assignments
                    .Where(a => a.Id == request.AssignmentId && a.Status != 0)
                    .ProjectTo<AssignmentGetAllAndByIdDto>(_mapper.ConfigurationProvider)
                    .FirstOrDefaultAsync(cancellationToken);

                // Periksa apakah assignment ditemukan
                if (assignmentDto == null)
                {
                    return Result<AssignmentGetAllAndByIdDto>.Failure("Assignment not found.");
                }

                return Result<AssignmentGetAllAndByIdDto>.Success(assignmentDto);
            }
        }
    }
}