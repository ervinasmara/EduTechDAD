using Application.Core;
using Application.Learn.GetFileName;
using MediatR;
using Persistence;

namespace Application.Tasks
{
    public class DownloadTask
    {
        public class Query : IRequest<Result<DownloadFileDto>>
        {
            public Guid Id { get; set; } // ID file yang akan diunduh
            public DownloadFileDto DownloadFileDto { get; set; }
        }

        public class Handler : IRequestHandler<Query, Result<DownloadFileDto>>
        {
            private readonly DataContext _context;

            public Handler(DataContext context)
            {
                _context = context;
            }

            public async Task<Result<DownloadFileDto>> Handle(Query request, CancellationToken cancellationToken)
            {
                var assignment = await _context.Assignments.FindAsync(request.Id);
                if (assignment == null)
                {
                    return Result<DownloadFileDto>.Failure("File not found.");
                }

                var fileExtension = GetFileExtension.FileExtensionHelper(assignment.FileData);
                if (fileExtension == null)
                {
                    return Result<DownloadFileDto>.Failure("File extension not recognized.");
                }

                var downloadFile = new DownloadFileDto
                {
                    FileData = assignment.FileData,
                    FileName = $"{assignment.AssignmentName}.{fileExtension}", // Menggunakan ekstensi file yang dideteksi
                    ContentType = GetContentType(fileExtension)
                };

                return Result<DownloadFileDto>.Success(downloadFile);
            }
            
            private string GetContentType(string fileExtension)
            {
                // Penetapan tipe konten berdasarkan ekstensi file
                switch (fileExtension.ToLower())
                {
                    case "jpg":
                    case "jpeg":
                        return "image/jpeg";
                    case "png":
                        return "image/png";
                    case "pdf":
                        return "application/pdf";
                    case "zip":
                        return "application/zip";
                    case "rar":
                        return "application/vnd.rar";
                    default:
                        return "application/octet-stream";
                }
            }
        }
    }
}