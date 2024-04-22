using Application.ClassRooms;
using Application.Core;
using Application.Learn.Lessons;
using Application.User.DTOs;
using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.User.Teachers
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
                    Status = teacher.Status,
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
                }).ToList();

                return Result<List<TeacherGetAllAndByIdDto>>.Success(teacherDtos);
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