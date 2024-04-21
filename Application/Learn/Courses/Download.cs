using Application.Core;
using Application.Learn.GetFileName;
using Application.Assignments;
using MediatR;
using Persistence;

namespace Application.Learn.Courses
{
    public class Download
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
                var course = await _context.Courses.FindAsync(request.Id);
                if (course == null)
                {
                    return Result<DownloadFileDto>.Failure("File not found.");
                }

                var fileExtension = GetFileExtension.FileExtensionHelper(course.FileData);
                if (fileExtension == null)
                {
                    return Result<DownloadFileDto>.Failure("File extension not recognized.");
                }

                var downloadFile = new DownloadFileDto
                {
                    FileData = course.FileData,
                    FileName = $"{course.CourseName}.{fileExtension}", // Menggunakan ekstensi file yang dideteksi
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