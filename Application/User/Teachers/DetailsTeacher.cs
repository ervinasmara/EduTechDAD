using Application.ClassRooms;
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
                    .Include(t => t.TeacherLessons)
                        .ThenInclude(tl => tl.Lesson)
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
                    LessonTeacher = teacher.TeacherLessons.Select(tl => new LessonTeacherIdGetDto
                    {
                        LessonName = tl.Lesson.LessonName,
                        ClassRooms = GetClassRoomsForLessonAndTeacher(tl.Lesson.Id, teacher.Id)
                    }).ToList()
                };

                return Result<TeacherGetAllDto>.Success(teacherDto);
            }

            private List<ClassRoomDto> GetClassRoomsForLessonAndTeacher(Guid lessonId, Guid teacherId)
            {
                return _context.TeacherClassRooms
                    .Where(tc => tc.TeacherId == teacherId && tc.ClassRoom.LessonClassRooms.Any(lcr => lcr.LessonId == lessonId))
                    .Select(tc => new ClassRoomDto
                    {
                        ClassName = tc.ClassRoom.ClassName,
                        UniqueNumberOfClassRoom = tc.ClassRoom.UniqueNumberOfClassRoom
                    })
                    .ToList();
            }
        }
    }
}