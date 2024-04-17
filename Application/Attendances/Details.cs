using Application.Core;
using Application.Attendances.DTOs;
using MediatR;
using Persistence;

namespace Application.Attendances
{
    public class Details
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