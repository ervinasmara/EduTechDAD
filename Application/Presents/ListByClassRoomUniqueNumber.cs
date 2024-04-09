using Application.Core;
using Application.Presents.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Presents
{
    public class ListByClassRoomUniqueNumber
    {
        public class Query : IRequest<Result<List<AttendanceDto>>>
        {
            public string UniqueNumberOfClassRoom { get; set; }
        }

        public class Handler : IRequestHandler<Query, Result<List<AttendanceDto>>>
        {
            private readonly DataContext _context;
            public Handler(DataContext context)
            {
                _context = context;
            }

            public async Task<Result<List<AttendanceDto>>> Handle(Query request, CancellationToken cancellationToken)
            {
                var classRoom = await _context.ClassRooms
                    .FirstOrDefaultAsync(c => c.UniqueNumberOfClassRoom == request.UniqueNumberOfClassRoom);

                if (classRoom == null)
                {
                    return Result<List<AttendanceDto>>.Failure("ClassRoom not found.");
                }

                var studentsInClassRoom = await _context.Students
                    .Where(s => s.ClassRoomId == classRoom.Id)
                    .ToListAsync(cancellationToken);

                var attendanceDtos = new List<AttendanceDto>();

                foreach (var student in studentsInClassRoom)
                {
                    var attendancesForStudent = await _context.Attendances
                        .Where(a => a.StudentId == student.Id)
                        .ToListAsync(cancellationToken);

                    attendanceDtos.AddRange(attendancesForStudent.Select(a => new AttendanceDto
                    {
                        Date = a.Date,
                        Status = a.Status,
                        StudentId = a.StudentId
                    }));
                }

                return Result<List<AttendanceDto>>.Success(attendanceDtos);
            }
        }
    }
}
