using Application.Core;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Attendances.Query;
public class DetailsAttendance
{
    public class Query : IRequest<Result<AttendanceGetByIdDto>>
    {
        public Guid AttendanceId { get; set; }
    }

    public class Handler : IRequestHandler<Query, Result<AttendanceGetByIdDto>>
    {
        private readonly DataContext _context;
        private readonly IMapper _mapper;

        public Handler(DataContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<Result<AttendanceGetByIdDto>> Handle(Query request, CancellationToken cancellationToken)
        {
            /** Langkah 1: Mengambil kehadiran berdasarkan ID **/
            var attendanceDto = await _context.Attendances
                .Where(a => a.Id == request.AttendanceId)
                .ProjectTo<AttendanceGetByIdDto>(_mapper.ConfigurationProvider)
                .SingleOrDefaultAsync(cancellationToken);

            /** Langkah 2: Memeriksa kehadiran ditemukan atau tidak **/
            if (attendanceDto == null)
                return Result<AttendanceGetByIdDto>.Failure("Kehadiran tidak ditemukan");

            /** Langkah 3: Mengembalikan hasil dalam bentuk Success Result **/
            return Result<AttendanceGetByIdDto>.Success(attendanceDto);
        }
    }
}