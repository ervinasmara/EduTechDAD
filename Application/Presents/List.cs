using Application.Core;
using Application.Presents.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Presents
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

            public Handler(DataContext context)
            {
                _context = context;
            }

            public async Task<Result<List<AttendanceGetDto>>> Handle(Query request, CancellationToken cancellationToken)
            {
                var attendancesWithDetails = await _context.Attendances
                    .Include(a => a.Student)
                        .ThenInclude(s => s.ClassRoom)
                    .ToListAsync(cancellationToken);

                var result = attendancesWithDetails.Select(a => new AttendanceGetDto
                {
                    Id = a.Id,
                    Date = a.Date,
                    Status = a.Status,
                    StudentAttendance = new StudentAttendanceDto
                    {
                        StudentId = a.Student.Id,
                        NameStudent = a.Student.NameStudent,
                    },
                    ClassRoomAttendance = new ClassRoomAttendanceDto
                    {
                        ClassRoomId = a.Student.ClassRoom.Id,
                        ClassName = a.Student.ClassRoom.ClassName,
                        UniqueNumberOfClassRoom = a.Student.ClassRoom.UniqueNumberOfClassRoom
                    }
                }).ToList();

                return Result<List<AttendanceGetDto>>.Success(result);
            }
        }
    }
}