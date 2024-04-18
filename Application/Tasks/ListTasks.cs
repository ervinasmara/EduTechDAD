using Application.Core;
using Application.Learn.GetFileName;
using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Tasks
{
    public class ListTasks
    {
        public class Query : IRequest<Result<List<AssignmentGetDto>>>
        {
            // Tidak memerlukan parameter tambahan untuk meneruskan ke query
        }

        public class Handler : IRequestHandler<Query, Result<List<AssignmentGetDto>>>
        {
            private readonly DataContext _context;
            private readonly IMapper _mapper;

            public Handler(DataContext context, IMapper mapper)
            {
                _context = context;
                _mapper = mapper;
            }

            public async Task<Result<List<AssignmentGetDto>>> Handle(Query request, CancellationToken cancellationToken)
            {
                var assignments = await _context.Assignments.Include(c => c.Course).ToListAsync(cancellationToken);
                var assignmentDtos = new List<AssignmentGetDto>();

                foreach (var assignment in assignments)
                {
                    var assignmentDto = _mapper.Map<AssignmentGetDto>(assignment);
                    assignmentDto.CourseName = assignment.Course.CourseName;

                    // Mengisi AssignmentFileData sesuai dengan data yang ada di database
                    assignmentDto.AssignmentFileData = assignment.FileData;

                    // Set AssignmentFileName based on AssignmentName and AssignmentFileData extension
                    if (!string.IsNullOrEmpty(assignment.AssignmentName) && assignment.FileData != null)
                    {
                        assignmentDto.AssignmentFileName = $"{assignment.AssignmentName}.{GetFileExtension.FileExtensionHelper(assignment.FileData)}";
                    }
                    else
                    {
                        // Handle null values appropriately
                        assignmentDto.AssignmentFileName = "UnknownFileName";
                    }

                    assignmentDtos.Add(assignmentDto);
                }

                return Result<List<AssignmentGetDto>>.Success(assignmentDtos);
            }
        }
    }
}