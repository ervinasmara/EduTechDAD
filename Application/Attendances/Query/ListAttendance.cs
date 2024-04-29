using Application.Core;
using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Attendances.Query
{
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
                var students = await _context.Students
                    .Include(s => s.ClassRoom) // Memuat data ruang kelas terkait dengan siswa
                    .ToListAsync(cancellationToken);

                // Mendapatkan semua siswa yang sudah absensi
                var attendances = await _context.Attendances.ToListAsync(cancellationToken);

                var groupedAttendances = students
                    .Select(student =>
                    {
                        var attendanceStudentDtos = attendances
                            .Where(a => a.StudentId == student.Id)
                            .Select(attendance => new AttendanceStudentDto
                            {
                                AttendanceId = attendance.Id,
                                Date = attendance.Date,
                                Status = attendance.Status
                            })
                            .ToList();

                        // Jika siswa belum absen, daftar kehadiran diisi dengan nilai null
                        if (attendanceStudentDtos.Count == 0)
                        {
                            attendanceStudentDtos = null;
                        }

                        return new AttendanceGetDto
                        {
                            StudentId = student.Id,
                            NameStudent = student.NameStudent,
                            UniqueNumberOfClassRoom = student.ClassRoom.UniqueNumberOfClassRoom,
                            AttendanceStudent = attendanceStudentDtos
                        };
                    })
                    .ToList();

                return Result<List<AttendanceGetDto>>.Success(groupedAttendances);
            }
        }
    }
}
