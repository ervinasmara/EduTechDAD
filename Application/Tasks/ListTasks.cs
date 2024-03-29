using Application.Core;
using AutoMapper;
using Domain.Task;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Tasks
{
    public class ListTasks
    {
        public class Query : IRequest<Result<List<AssignmentGetAllDto>>>
        {
            // Tidak memerlukan parameter tambahan untuk meneruskan ke query
        }

        public class Handler : IRequestHandler<Query, Result<List<AssignmentGetAllDto>>>
        {
            private readonly DataContext _context;
            private readonly IMapper _mapper;

            public Handler(DataContext context, IMapper mapper)
            {
                _context = context;
                _mapper = mapper;
            }

            public async Task<Result<List<AssignmentGetAllDto>>> Handle(Query request, CancellationToken cancellationToken)
            {
                var assignments = await _context.Assignments.Include(c => c.Course).ToListAsync(cancellationToken);
                var assignmentDtos = new List<AssignmentGetAllDto>();

                foreach (var assignment in assignments)
                {
                    var assignmentDto = _mapper.Map<AssignmentGetAllDto>(assignment);
                    assignmentDto.CourseName = assignment.Course.CourseName;
                    assignmentDtos.Add(assignmentDto);
                }

                return Result<List<AssignmentGetAllDto>>.Success(assignmentDtos);
            }
        }
    }
}