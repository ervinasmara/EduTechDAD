using Application.Core;
using Application.User.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.User.Students
{
    public class DetailsStudent
    {
        public class Query : IRequest<Result<List<StudentGetByIdDto>>>
        {
            public string Nis { get; set; }
            public string Name { get; set; }
            public string ClassName { get; set; }
        }

        public class Handler : IRequestHandler<Query, Result<List<StudentGetByIdDto>>>
        {
            private readonly DataContext _context;

            public Handler(DataContext context)
            {
                _context = context;
            }

            public async Task<Result<List<StudentGetByIdDto>>> Handle(Query request, CancellationToken cancellationToken)
            {
                IQueryable<Domain.User.Student> query = _context.Students
                    .Include(s => s.User)
                    .Include(s => s.ClassRoom);

                if (!string.IsNullOrEmpty(request.Nis))
                {
                    query = query.Where(s => EF.Functions.Like(s.Nis, $"%{request.Nis}%"));
                }

                if (!string.IsNullOrEmpty(request.Name))
                {
                    query = query.Where(s => s.NameStudent.ToLower().Contains(request.Name.ToLower()));
                }

                if (!string.IsNullOrEmpty(request.ClassName))
                {
                    query = query.Where(s => s.ClassRoom.ClassName.ToLower().Contains(request.ClassName.ToLower()));
                }

                if (string.IsNullOrEmpty(request.Nis) && string.IsNullOrEmpty(request.Name) && string.IsNullOrEmpty(request.ClassName))
                {
                    return Result<List<StudentGetByIdDto>>.Failure("Please provide search criteria.");
                }

                var students = await query.ToListAsync();

                if (students.Count == 0)
                {
                    return Result<List<StudentGetByIdDto>>.Failure("No students found.");
                }

                var studentDtos = students.Select(student => new StudentGetByIdDto
                {
                    NameStudent = student.NameStudent,
                    Nis = student.Nis,
                    BirthDate = student.BirthDate,
                    BirthPlace = student.BirthPlace,
                    Address = student.Address,
                    PhoneNumber = student.PhoneNumber,
                    ParentName = student.ParentName,
                    Username = student.User?.UserName ?? "No Username",
                    ClassRoomId = student.ClassRoom.Id,
                    ClassName = student.ClassRoom?.ClassName ?? "No Class",
                    Gender = student.Gender
                }).ToList();

                return Result<List<StudentGetByIdDto>>.Success(studentDtos);
            }
        }
    }
}