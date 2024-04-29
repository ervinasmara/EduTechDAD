using Application.Core;
using MediatR;
using Persistence;

namespace Application.Attendances.Query
{
    public class DetailsAttendance
    {
        public class Query : IRequest<Result<AttendanceGetByIdDto>>
        {
            public Guid Id { get; set; }
        }

        public class Handler : IRequestHandler<Query, Result<AttendanceGetByIdDto>>
        {
            private readonly DataContext _context;

            public Handler(DataContext context)
            {
                _context = context;
            }

            public async Task<Result<AttendanceGetByIdDto>> Handle(Query request, CancellationToken cancellationToken)
            {
                var attendance = await _context.Attendances.FindAsync(request.Id);

                if (attendance == null)
                    return Result<AttendanceGetByIdDto>.Failure("Attendance not found.");

                var attendanceDto = new AttendanceGetByIdDto
                {
                    Id = attendance.Id,
                    Date = attendance.Date,
                    Status = attendance.Status,
                    StudentId = attendance.StudentId
                };

                return Result<AttendanceGetByIdDto>.Success(attendanceDto);
            }
        }
    }
}