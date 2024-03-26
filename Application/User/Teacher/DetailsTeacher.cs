using Application.Core;
using Domain.User.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.User.Teacher
{
    public class DetailsTeacher
    {
        public class Query : IRequest<Result<TeacherGetByIdDto>>
        {
            public Guid Id { get; set; }
        }

        public class Handler : IRequestHandler<Query, Result<TeacherGetByIdDto>>
        {
            private readonly DataContext _context;

            public Handler(DataContext context)
            {
                _context = context;
            }

            public async Task<Result<TeacherGetByIdDto>> Handle(Query request, CancellationToken cancellationToken)
            {
                var student = await _context.Teachers
                    .Include(s => s.User)
                    .FirstOrDefaultAsync(s => s.Id == request.Id);

                if (student == null)
                {
                    return Result<TeacherGetByIdDto>.Failure("Teacher not found.");
                }

                var studentDto = new TeacherGetByIdDto
                {
                    NameTeacher = student.NameTeacher,
                    Nip = student.Nip,
                    BirthDate = student.BirthDate,
                    BirthPlace = student.BirthPlace,
                    Address = student.Address,
                    PhoneNumber = student.PhoneNumber,
                    Username = student.User?.UserName ?? "No Username",
                };

                return Result<TeacherGetByIdDto>.Success(studentDto);
            }
        }
    }
}