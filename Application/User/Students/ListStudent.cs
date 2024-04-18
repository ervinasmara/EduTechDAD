using Application.Core;
using Application.User.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.User.Students
{
    public class ListStudent
    {
        public class Query : IRequest<Result<List<StudentGetAllDto>>>
        {
            /* Kita tidak memerlukan parameter tambahan untuk meneruskan ke query*/
        }

        public class Handler : IRequestHandler<Query, Result<List<StudentGetAllDto>>>
        {
            private readonly DataContext _context;

            public Handler(DataContext context)
            {
                _context = context;
            }

            public async Task<Result<List<StudentGetAllDto>>> Handle(Query request, CancellationToken cancellationToken)
            {
                var students = await _context.Students
                    .Select(s => new StudentGetAllDto
                    {
                        Id = s.Id,
                        NameStudent = s.NameStudent,
                        Nis = s.Nis,
                        BirthDate = s.BirthDate,
                        BirthPlace = s.BirthPlace,
                        Address = s.Address,
                        PhoneNumber = s.PhoneNumber,
                        ParentName = s.ParentName,
                        Username = s.User.UserName,
                        Role = s.User.Role,
                        Gender = s.Gender,
                        ClassRoomId = s.ClassRoom.Id,
                        ClassName = s.ClassRoom.ClassName,
                        UniqueNumberOfClassRoom = s.ClassRoom.UniqueNumberOfClassRoom
                    })
                    .ToListAsync(cancellationToken);

                return Result<List<StudentGetAllDto>>.Success(students);
            }
        }
    }
}