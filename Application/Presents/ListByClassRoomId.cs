using Application.Core;
using Domain.Present;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Presents
{
    public class ListByClassRoomId
    {
        public class Query : IRequest<Result<List<AttendanceDto>>>
        {
            public Guid ClassRoomId { get; set; }
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
                var studentsInClassRoom = await _context.Students
                    .Where(s => s.ClassRoomId == request.ClassRoomId)
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