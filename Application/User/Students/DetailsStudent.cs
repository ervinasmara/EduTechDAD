using Application.Core;
using Application.User.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.User.Students
{
    public class DetailsStudent
    {
        public class Query : IRequest<Result<StudentGetByIdDto>>
        {
            public Guid StudentId { get; set; }
        }

        public class Handler : IRequestHandler<Query, Result<StudentGetByIdDto>>
        {
            private readonly DataContext _context;

            public Handler(DataContext context)
            {
                _context = context;
            }

            public async Task<Result<StudentGetByIdDto>> Handle(Query request, CancellationToken cancellationToken)
            {
                var student = await _context.Students
                    .Include(s => s.User)
                    .Include(s => s.ClassRoom)
                    .FirstOrDefaultAsync(s => s.Id == request.StudentId);

                if (student == null)
                {
                    return Result<StudentGetByIdDto>.Failure("Student not found.");
                }

                var studentDto = new StudentGetByIdDto
                {
                    Status = student.Status == 1 ? "IsActive" : "NotActive",
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
                    UniqueNumberOfClassRoom = student.ClassRoom?.UniqueNumberOfClassRoom,
                    Gender = student.Gender
                };

                return Result<StudentGetByIdDto>.Success(studentDto);
            }
        }
    }
}