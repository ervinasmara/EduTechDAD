using Application.Assignments;
using Application.Core;
using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Submission.Query;
public class DownloadSubmission
{
    public class Query : IRequest<Result<DownloadFileDto>>
    {
        public Guid SubmissionId { get; set; } // ID file yang akan diunduh
    }

    public class Handler : IRequestHandler<Query, Result<DownloadFileDto>>
    {
        private readonly DataContext _context;
        private readonly IMapper _mapper;

        public Handler(DataContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }
        public async Task<Result<DownloadFileDto>> Handle(Query request, CancellationToken cancellationToken)
        {
            var submission = await _context.AssignmentSubmissions
                .Include(s => s.Student) // Menyertakan entitas Student
                .Include(s => s.Assignment) // Menyertakan entitas Assignment
                .FirstOrDefaultAsync(s => s.Id == request.SubmissionId);

            if (submission == null)
            {
                return Result<DownloadFileDto>.Failure("File not found.");
            }

            // AutoMapper akan menangani pembacaan file dan penentuan ContentType
            var downloadFileDto = _mapper.Map<DownloadFileDto>(submission);

            if (downloadFileDto.FileData == null)
            {
                return Result<DownloadFileDto>.Failure("File not found.");
            }

            return Result<DownloadFileDto>.Success(downloadFileDto);
        }
    }
}