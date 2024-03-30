using Application.Core;
using Domain.User.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.User.Teacher
{
    public class ListTeacher
    {
        public class Query : IRequest<Result<List<TeacherGetAllDto>>>
        {
            /* Kita tidak memerlukan parameter tambahan untuk meneruskan ke query*/
        }

        public class Handler : IRequestHandler<Query, Result<List<TeacherGetAllDto>>>
        {
            private readonly DataContext _context;

            public Handler(DataContext context)
            {
                _context = context;
            }

            public async Task<Result<List<TeacherGetAllDto>>> Handle(Query request, CancellationToken cancellationToken)
            {
                var students = await _context.Teachers
                    .Select(s => new TeacherGetAllDto
                    {
                        Id = s.Id,
                        NameTeacher = s.NameTeacher,
                        BirthDate = s.BirthDate,
                        BirthPlace = s.BirthPlace,
                        Address = s.Address,
                        PhoneNumber = s.PhoneNumber,
                        Nip = s.Nip,
                    })
                    .ToListAsync(cancellationToken);

                return Result<List<TeacherGetAllDto>>.Success(students);
            }
        }
    }
}