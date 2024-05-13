using Application.Core;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Attendances.Query;
public class ListAttendance
{
    public class Query : IRequest<Result<List<AttendanceGetDto>>>
    {
        // Tidak ada parameter tambahan yang diperlukan
    }

    public class Handler : IRequestHandler<Query, Result<List<AttendanceGetDto>>>
    {
        private readonly DataContext _context;
        private readonly IMapper _mapper;

        public Handler(DataContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<Result<List<AttendanceGetDto>>> Handle(Query request, CancellationToken cancellationToken)
        {
            /** Langkah 1: Mendapatkan Query awal untuk siswa **/
            var studentsQuery = _context.Students
                .Include(s => s.ClassRoom)
                .Include(s => s.Attendances)
                .Where(s => s.Status != 0) // Menyembunyikan siswa dengan status 0
                .OrderBy(s => s.Nis)
                .AsQueryable();

            /** Langkah 2: Proyeksi ke AttendanceGetDto menggunakan AutoMapper **/
            var attendanceDtos = await studentsQuery
                .ProjectTo<AttendanceGetDto>(_mapper.ConfigurationProvider)
                .ToListAsync(cancellationToken);

            /** Langkah 3: Menyaring siswa yang tidak memiliki kehadiran **/
            attendanceDtos.ForEach(dto =>
            {
                if (dto.AttendanceStudent != null && dto.AttendanceStudent.Count == 0)
                {
                    dto.AttendanceStudent = null;
                }
            });

            /** Langkah 4: Mengembalikan hasil dalam bentuk Success Result **/
            return Result<List<AttendanceGetDto>>.Success(attendanceDtos);
        }
    }
}