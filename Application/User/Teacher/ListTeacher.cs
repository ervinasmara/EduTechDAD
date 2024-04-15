using Application.Core;
using Application.Learn.Lessons;
using Application.User.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.User.Teacher
{
    public class ListTeacher
    {
        public class Query : IRequest<Result<List<TeacherGetAllDto>>>
        {
            // Tidak memerlukan parameter tambahan untuk meneruskan ke query
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
                var teachers = await _context.Teachers
                    .Include(t => t.Lessons) // Memuat data lesson yang terkait dengan setiap guru
                    .Select(t => new TeacherGetAllDto
                    {
                        Id = t.Id,
                        NameTeacher = t.NameTeacher,
                        BirthDate = t.BirthDate,
                        BirthPlace = t.BirthPlace,
                        Address = t.Address,
                        PhoneNumber = t.PhoneNumber,
                        Nip = t.Nip,
                        LessonTeacher = t.Lessons.Select(l => new LessonGetTeacherDto
                        {
                            Id = l.Id,
                            LessonName = l.LessonName,
                            UniqueNumberOfLesson = l.UniqueNumberOfLesson
                        }).ToList()
                    })
                    .ToListAsync(cancellationToken);

                return Result<List<TeacherGetAllDto>>.Success(teachers);
            }
        }
    }
}