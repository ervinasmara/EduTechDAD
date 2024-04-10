using Application.Core;
using Application.Presents.DTOs;
using Domain.Present;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Presents
{
    public class Details
    {
        public class Query : IRequest<Result<AttendanceGetDto>>
        {
            public Guid Id { get; set; }
        }

        public class Handler : IRequestHandler<Query, Result<AttendanceGetDto>>
        {
            private readonly DataContext _context;

            public Handler(DataContext context)
            {
                _context = context;
            }

            public async Task<Result<AttendanceGetDto>> Handle(Query request, CancellationToken cancellationToken)
            {
                var attendance = await _context.Attendances
                    .Include(a => a.Student)
                        .ThenInclude(s => s.ClassRoom)
                    .FirstOrDefaultAsync(a => a.Id == request.Id, cancellationToken);

                if (attendance == null)
                    return Result<AttendanceGetDto>.Failure("Attendance not found.");

                var result = new AttendanceGetDto
                {
                    Id = attendance.Id,
                    Date = attendance.Date,
                    Status = attendance.Status,
                    StudentAttendance = new StudentAttendanceDto
                    {
                        StudentId = attendance.Student.Id,
                        NameStudent = attendance.Student.NameStudent,
                    },
                    ClassRoomAttendance = new ClassRoomAttendanceDto
                    {
                        ClassRoomId = attendance.Student.ClassRoom.Id,
                        ClassName = attendance.Student.ClassRoom.ClassName,
                        UniqueNumberOfClassRoom = attendance.Student.ClassRoom.UniqueNumberOfClassRoom
                    }
                };

                return Result<AttendanceGetDto>.Success(result);
            }
        }
    }
}