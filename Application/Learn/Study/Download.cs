using Application.Core;
using MediatR;
using Persistence;

namespace Application.Learn.Study
{
    public class Download
    {
        public class Query : IRequest<Result<DownloadFile>>
        {
            public Guid Id { get; set; } // ID file yang akan diunduh
        }

        public class DownloadFile
        {
            public byte[] FileData { get; set; }
            public string FileName { get; set; }
        }

        public class Handler : IRequestHandler<Query, Result<DownloadFile>>
        {
            private readonly DataContext _context;

            public Handler(DataContext context)
            {
                _context = context;
            }

            public async Task<Result<DownloadFile>> Handle(Query request, CancellationToken cancellationToken)
            {
                var course = await _context.Courses.FindAsync(request.Id);
                if (course == null)
                {
                    return Result<DownloadFile>.Failure("File not found.");
                }

                var fileExtension = GetFileExtension(course.FileData);
                if (fileExtension == null)
                {
                    return Result<DownloadFile>.Failure("File extension not recognized.");
                }

                var downloadFile = new DownloadFile
                {
                    FileData = course.FileData,
                    FileName = $"{course.CourseName}.{fileExtension}" // Menggunakan ekstensi file yang dideteksi
                };

                return Result<DownloadFile>.Success(downloadFile);
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
                // Tambahkan kondisi lain sesuai dengan jenis file yang ingin didukung
                else
                {
                    return null; // Ekstensi file tidak dikenali
                }
            }
        }
    }
}