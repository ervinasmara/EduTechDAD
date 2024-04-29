using Application.Core;
using Application.User.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.User.Teachers.Query
{
    public class ListTeacher
    {
        public class Query : IRequest<Result<List<TeacherGetAllAndByIdDto>>>
        {
            // Tidak memerlukan parameter tambahan untuk meneruskan ke query
        }

        public class Handler : IRequestHandler<Query, Result<List<TeacherGetAllAndByIdDto>>>
        {
            private readonly DataContext _context;

            public Handler(DataContext context)
            {
                _context = context;
            }

            public async Task<Result<List<TeacherGetAllAndByIdDto>>> Handle(Query request, CancellationToken cancellationToken)
            {
                var teachers = await _context.Teachers
                    .Include(t => t.TeacherLessons)
                        .ThenInclude(tl => tl.Lesson)
                    .ToListAsync();

                if (teachers == null || !teachers.Any())
                {
                    return Result<List<TeacherGetAllAndByIdDto>>.Failure("No teachers found.");
                }

                var teacherDtos = teachers.Select(teacher => new TeacherGetAllAndByIdDto
                {
                    Id = teacher.Id,
                    Status = teacher.Status == 1 ? "IsActive" : "NotActive",
                    NameTeacher = teacher.NameTeacher,
                    BirthDate = teacher.BirthDate,
                    BirthPlace = teacher.BirthPlace,
                    Address = teacher.Address,
                    PhoneNumber = teacher.PhoneNumber,
                    Nip = teacher.Nip,
                    Gender = teacher.Gender == 1 ? "Laki - Laki" : "Perempuan",
                    LessonNames = teacher.TeacherLessons.Select(tl => tl.Lesson.LessonName).ToList(),
                    ClassNames = GetClassNamesForTeacher(teacher.Id)
                }).ToList();

                return Result<List<TeacherGetAllAndByIdDto>>.Success(teacherDtos);
            }

            private ICollection<string> GetClassNamesForTeacher(Guid teacherId)
            {
                var classRooms = _context.TeacherLessons
                    .Where(tl => tl.TeacherId == teacherId)
                    .SelectMany(tl => tl.Lesson.ClassRoom.Lessons.Select(lcr => lcr.ClassRoom.ClassName))
                    .Distinct() // Hapus duplikat kelas
                    .ToList();

                return classRooms;
            }
        }
    }
}