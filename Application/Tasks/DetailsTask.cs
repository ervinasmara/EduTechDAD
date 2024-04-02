using Application.Core;
using AutoMapper;
using Domain.Task;
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
                    assignmentDto.AssignmentFileName = $"{assignment.AssignmentName}.{GetFileExtension(assignment.FileData)}";
                }
                else
                {
                    // Handle null values appropriately
                    assignmentDto.AssignmentFileName = "UnknownFileName";
                }

                return Result<AssignmentGetDto>.Success(assignmentDto);
            }

            private string GetFileExtension(byte[] fileData)
            {
                if (fileData == null || fileData.Length < 4)
                    return null;

                // Analisis byte pertama untuk menentukan jenis file
                if (fileData[0] == 0xFF && fileData[1] == 0xD8 && fileData[2] == 0xFF)
                {
                    return "jpg";
                }
                else if (fileData[0] == 0x89 && fileData[1] == 0x50 && fileData[2] == 0x4E && fileData[3] == 0x47)
                {
                    return "png";
                }
                else if (fileData[0] == 0x25 && fileData[1] == 0x50 && fileData[2] == 0x44 && fileData[3] == 0x46)
                {
                    return "pdf";
                }
                else if (fileData[0] == 0x50 && fileData[1] == 0x4B && fileData[2] == 0x03 && fileData[3] == 0x04)
                {
                    return "zip";
                }
                else if (fileData[0] == 0x52 && fileData[1] == 0x61 && fileData[2] == 0x72 && fileData[3] == 0x21)
                {
                    return "rar";
                }
                else
                {
                    return null; // Ekstensi file tidak dikenali
                }
            }
        }
    }
}