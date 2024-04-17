using Application.Core;
using Application.Attendances.DTOs;
using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Attendances
{
    public class List
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
                var attendances = await _context.Attendances
                    .Include(a => a.Student) // Memuat data siswa terkait
                        .ThenInclude(s => s.ClassRoom) // Memuat data ruang kelas terkait dengan siswa
                    .ToListAsync(cancellationToken);

                var groupedAttendances = attendances
                    .GroupBy(a => a.StudentId)
                    .Select(group => new AttendanceGetDto
                    {
                        StudentId = group.Key,
                        NameStudent = group.First().Student.NameStudent, // Mendapatkan nama siswa
                        UniqueNumberOfClassRoom = group.First().Student.ClassRoom.UniqueNumberOfClassRoom, // Mendapatkan nomor unik ruang kelas
                        AttendanceStudent = group.Select(attendance => new AttendanceStudentDto
                        {
                            AttendanceId = attendance.Id,
                            Date = attendance.Date,
                            Status = attendance.Status
                        }).ToList()
                    })
                    .ToList();

                return Result<List<AttendanceGetDto>>.Success(groupedAttendances);
            }
        }
    }
}