using Application.Assignments;
using Application.Core;
using AutoMapper;
using Domain.Learn.Courses;
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
                return Result<DownloadFileDto>.Failure("File tidak ditemukan");
            }

            // Pastikan path file benar dalam container
            var filePath = submission.FilePath;
            if (!System.IO.File.Exists(filePath))
            {
                return Result<DownloadFileDto>.Failure("File tidak ditemukan");
            }

            var downloadFileDto = _mapper.Map<DownloadFileDto>(submission);
            downloadFileDto.FileData = await System.IO.File.ReadAllBytesAsync(filePath);
            downloadFileDto.FileName = Path.GetFileName(filePath);
            downloadFileDto.ContentType = "application/pdf"; // Sesuaikan ContentType jika perlu

            return Result<DownloadFileDto>.Success(downloadFileDto);
        }
    }
}