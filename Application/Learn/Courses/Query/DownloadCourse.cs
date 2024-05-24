using Application.Core;
using Application.Assignments;
using MediatR;
using Persistence;
using AutoMapper;

namespace Application.Learn.Courses.Query;
public class DownloadCourse
{
    public class Query : IRequest<Result<DownloadFileDto>>
    {
        public Guid CourseId { get; set; } // ID file yang akan diunduh
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
            var course = await _context.Courses.FindAsync(request.CourseId);
            if (course == null)
            {
                return Result<DownloadFileDto>.Failure("File tidak ditemukan");
            }

	        // Pastikan path file benar dalam container
            var filePath = course.FilePath;
            if (!System.IO.File.Exists(filePath))
            {
                return Result<DownloadFileDto>.Failure("File tidak ditemukan");
            }

            var downloadFileDto = _mapper.Map<DownloadFileDto>(course);
            downloadFileDto.FileData = await System.IO.File.ReadAllBytesAsync(filePath);
            downloadFileDto.FileName = Path.GetFileName(filePath);
            downloadFileDto.ContentType = "application/pdf"; // Sesuaikan ContentType jika perlu

            return Result<DownloadFileDto>.Success(downloadFileDto);
        }
    }
}
