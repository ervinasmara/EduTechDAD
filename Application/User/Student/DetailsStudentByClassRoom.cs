using Application.Core;
using Domain.User.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.User.Student
{
    public class DetailsStudentByClassRoom
    {
        public class Query : IRequest<Result<List<StudentGetByIdDto>>>
        {
            public string UniqueNumberOfClassRoom { get; set; }
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
                var students = await _context.Students
                    .Include(s => s.User)
                    .Include(s => s.ClassRoom)
                    .Where(s => s.ClassRoom.UniqueNumberOfClassRoom == request.UniqueNumberOfClassRoom)
                    .ToListAsync(cancellationToken);

                if (students == null || !students.Any())
                {
                    return Result<List<StudentGetByIdDto>>.Failure("Classroom not found.");
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