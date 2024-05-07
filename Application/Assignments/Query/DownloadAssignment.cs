using Application.Core;
using AutoMapper;
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

            // AutoMapper akan menangani pembacaan file dan penentuan ContentType
            var downloadFileDto = _mapper.Map<DownloadFileDto>(assignment);

            if (downloadFileDto.FileData == null)
            {
                return Result<DownloadFileDto>.Failure("File tidak ditemukan");
            }

            return Result<DownloadFileDto>.Success(downloadFileDto);
        }
    }
}