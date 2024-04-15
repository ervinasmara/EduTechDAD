using Application.Core;
using Application.Attendances.DTOs;
using Domain.Attendances;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Attendances
{
    public class GetAttendanceByFilter
    {
        public class Query : IRequest<Result<List<AttendanceGetDto>>>
        {
            public int? Year { get; set; }
            public int? Month { get; set; }
            public int? Day { get; set; }
            public string NameStudent { get; set; }
            public string ClassName { get; set; }
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
                IQueryable<Attendance> query = _context.Attendances
                    .Include(a => a.Student)
                        .ThenInclude(s => s.ClassRoom);

                if (request.Year != null)
                {
                    query = query.Where(a => a.Date.Year == request.Year);
                }

                if (request.Month != null)
                {
                    query = query.Where(a => a.Date.Month == request.Month);
                }

                if (request.Day != null)
                {
                    query = query.Where(a => a.Date.Day == request.Day);
                }

                if (!string.IsNullOrWhiteSpace(request.NameStudent))
                {
                    string nameStudent = request.NameStudent.ToLower();
                    query = query.Where(a => a.Student.NameStudent.ToLower().Contains(nameStudent));
                }

                if (!string.IsNullOrWhiteSpace(request.ClassName))
                {
                    string className = request.ClassName.ToLower();
                    query = query.Where(a => a.Student.ClassRoom.ClassName.ToLower().Contains(className));
                }

                var attendances = await query.ToListAsync(cancellationToken);

                if (attendances == null || attendances.Count == 0)
                    return Result<List<AttendanceGetDto>>.Failure("No attendance found for the specified filter.");

                var result = attendances.Select(a => new AttendanceGetDto
                {
                    Id = a.Id,
                    Date = a.Date,
                    Status = a.Status,
                    StudentId = a.StudentId
                }).ToList();

                return Result<List<AttendanceGetDto>>.Success(result);
            }
        }
    }
}