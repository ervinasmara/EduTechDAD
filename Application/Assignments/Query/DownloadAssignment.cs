using Application.Core;
using AutoMapper;
using Domain.Learn.Courses;
using MediatR;
using Persistence;

namespace Application.Assignments.Query;
public class DownloadAssignment
{
    public class Query : IRequest<Result<DownloadFileDto>>
    {
        public Guid AssignmentId { get; set; } // ID file yang akan diunduh
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
            var assignment = await _context.Assignments.FindAsync(request.AssignmentId);
            if (assignment == null)
            {
                return Result<DownloadFileDto>.Failure("File tidak ditemukan");
            }

            // Pastikan path file benar dalam container
            var filePath = assignment.FilePath;
            if (!System.IO.File.Exists(filePath))
            {
                return Result<DownloadFileDto>.Failure("File tidak ditemukan");
            }

            var downloadFileDto = _mapper.Map<DownloadFileDto>(assignment);
            downloadFileDto.FileData = await System.IO.File.ReadAllBytesAsync(filePath);
            downloadFileDto.FileName = Path.GetFileName(filePath);
            downloadFileDto.ContentType = "application/pdf"; // Sesuaikan ContentType jika perlu

            return Result<DownloadFileDto>.Success(downloadFileDto);
        }
    }
}