using Application.Core;
using Application.Assignments;
using MediatR;
using Persistence;
using AutoMapper;

namespace Application.Learn.Courses.Query
{
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
                    return Result<DownloadFileDto>.Failure("File not found.");
                }

                // AutoMapper akan menangani pembacaan file dan penentuan ContentType
                var downloadFileDto = _mapper.Map<DownloadFileDto>(course);

                if (downloadFileDto.FileData == null)
                {
                    return Result<DownloadFileDto>.Failure("File not found.");
                }

                return Result<DownloadFileDto>.Success(downloadFileDto);
            }
        }
    }
}