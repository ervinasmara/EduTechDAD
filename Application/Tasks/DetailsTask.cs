using Application.Core;
using Application.Learn.GetFileName;
using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Tasks
{
    public class DetailsTask
    {
        public class Query : IRequest<Result<AssignmentGetDto>>
        {
            public Guid Id { get; set; }
        }

        public class Handler : IRequestHandler<Query, Result<AssignmentGetDto>>
        {
            private readonly DataContext _context;
            private readonly IMapper _mapper;

            public Handler(DataContext context, IMapper mapper)
            {
                _context = context;
                _mapper = mapper;
            }

            public async Task<Result<AssignmentGetDto>> Handle(Query request, CancellationToken cancellationToken)
            {
                var assignment = await _context.Assignments.Include(c => c.Course).FirstOrDefaultAsync(a => a.Id == request.Id, cancellationToken);

                if (assignment == null)
                    return Result<AssignmentGetDto>.Failure("Assignment not found.");

                var assignmentDto = _mapper.Map<AssignmentGetDto>(assignment);
                assignmentDto.CourseName = assignment.Course.CourseName;

                // Set AssignmentFileName based on AssignmentName and FileData extension
                if (!string.IsNullOrEmpty(assignment.AssignmentName) && assignment.FileData != null)
                {
                    assignmentDto.AssignmentFileName = $"{assignment.AssignmentName}.{GetFileExtension.FileExtensionHelper(assignment.FileData)}";
                }
                else
                {
                    // Handle null values appropriately
                    assignmentDto.AssignmentFileName = "UnknownFileName";
                }

                return Result<AssignmentGetDto>.Success(assignmentDto);
            }
        }
    }
}