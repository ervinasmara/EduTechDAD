using Application.Core;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Assignments.Query
{
    public class ListAssignments
    {
        public class Query : IRequest<Result<List<AssignmentGetAllAndByIdDto>>>
        {
            // Tidak memerlukan parameter tambahan untuk meneruskan ke query
        }

        public class Handler : IRequestHandler<Query, Result<List<AssignmentGetAllAndByIdDto>>>
        {
            private readonly DataContext _context;
            private readonly IMapper _mapper;

            public Handler(DataContext context, IMapper mapper)
            {
                _context = context;
                _mapper = mapper;
            }

            public async Task<Result<List<AssignmentGetAllAndByIdDto>>> Handle(Query request, CancellationToken cancellationToken)
            {
                try
                {
                    // Dapatkan semua tugas (assignments)
                    var assignments = await _context.Assignments
                        .OrderByDescending(a => a.CreatedAt)
                        .ProjectTo<AssignmentGetAllAndByIdDto>(_mapper.ConfigurationProvider)
                        .ToListAsync(cancellationToken);

                    if (assignments == null || !assignments.Any())
                    {
                        return Result<List<AssignmentGetAllAndByIdDto>>.Failure("No assignments found for the given TeacherId");
                    }

                    return Result<List<AssignmentGetAllAndByIdDto>>.Success(assignments);
                }
                catch (Exception ex)
                {
                    // Tangani pengecualian dan kembalikan pesan kesalahan yang sesuai
                    return Result<List<AssignmentGetAllAndByIdDto>>.Failure($"Failed to retrieve assignments: {ex.Message}");
                }
            }
        }
    }
}