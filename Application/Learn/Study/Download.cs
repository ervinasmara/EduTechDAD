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

                var downloadFile = new DownloadFile
                {
                    FileData = course.FileData,
                    FileName = $"{course.CourseName}.pdf" // Nama file yang diunduh berdasarkan CourseName
                };

                return Result<DownloadFile>.Success(downloadFile);
            }
        }
    }
}