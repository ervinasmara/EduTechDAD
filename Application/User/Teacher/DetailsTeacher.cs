using Application.Core;
using Application.Learn.Lessons;
using Application.User.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.User.Teacher
{
    public class DetailsTeacher
    {
        public class Query : IRequest<Result<TeacherGetAllDto>>
        {
            public Guid Id { get; set; }
        }

        public class Handler : IRequestHandler<Query, Result<TeacherGetAllDto>>
        {
            private readonly DataContext _context;

            public Handler(DataContext context)
            {
                _context = context;
            }

            public async Task<Result<TeacherGetAllDto>> Handle(Query request, CancellationToken cancellationToken)
            {
                var teacher = await _context.Teachers
                    .Include(t => t.Lessons) // Memuat data lesson yang terkait dengan guru
                    .FirstOrDefaultAsync(t => t.Id == request.Id);

                if (teacher == null)
                {
                    return Result<TeacherGetAllDto>.Failure("Teacher not found.");
                }

                var teacherDto = new TeacherGetAllDto
                {
                    Id = teacher.Id,
                    NameTeacher = teacher.NameTeacher,
                    BirthDate = teacher.BirthDate,
                    BirthPlace = teacher.BirthPlace,
                    Address = teacher.Address,
                    PhoneNumber = teacher.PhoneNumber,
                    Nip = teacher.Nip,
                    LessonTeacher = teacher.Lessons.Select(l => new LessonGetTeacherDto
                    {
                        Id = l.Id,
                        LessonName = l.LessonName,
                        UniqueNumberOfLesson = l.UniqueNumberOfLesson
                    }).ToList()
                };

                return Result<TeacherGetAllDto>.Success(teacherDto);
            }
        }
    }
}