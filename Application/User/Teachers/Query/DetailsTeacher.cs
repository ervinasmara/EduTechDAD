using Application.Core;
using Application.User.DTOs;
using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.User.Teachers.Query
{
    public class DetailsTeacher
    {
        public class Query : IRequest<Result<TeacherGetAllAndByIdDto>>
        {
            public Guid TeacherId { get; set; }
        }

        public class Handler : IRequestHandler<Query, Result<TeacherGetAllAndByIdDto>>
        {
            private readonly DataContext _context;
            private readonly IMapper _mapper;

            public Handler(DataContext context, IMapper mapper)
            {
                _context = context;
                _mapper = mapper;
            }

            public async Task<Result<TeacherGetAllAndByIdDto>> Handle(Query request, CancellationToken cancellationToken)
            {
                var teacher = await _context.Teachers
                    .Include(t => t.TeacherLessons)
                        .ThenInclude(tl => tl.Lesson)
                    .FirstOrDefaultAsync(t => t.Id == request.TeacherId);

                if (teacher == null)
                    return Result<TeacherGetAllAndByIdDto>.Failure("Teacher not found.");

                var teacherDto = _mapper.Map<TeacherGetAllAndByIdDto>(teacher);

                teacherDto.Status = teacher.Status == 1 ? "IsActive" : "NotActive";

                // Get all lesson names for the teacher
                teacherDto.LessonNames = GetLessonNamesForTeacher(teacher.Id);

                // Get all class names for the teacher
                teacherDto.ClassNames = GetClassNamesForTeacher(teacher.Id);

                return Result<TeacherGetAllAndByIdDto>.Success(teacherDto);
            }

            private ICollection<string> GetLessonNamesForTeacher(Guid teacherId)
            {
                var lessonNames = _context.TeacherLessons
                    .Where(tl => tl.TeacherId == teacherId)
                    .Select(tl => tl.Lesson.LessonName)
                    .Distinct()
                    .ToList();

                return lessonNames;
            }

            private ICollection<string> GetClassNamesForTeacher(Guid teacherId)
            {
                var classRooms = _context.TeacherLessons
                    .Where(tl => tl.TeacherId == teacherId)
                    .SelectMany(tl => tl.Lesson.ClassRoom.Lessons.Select(lcr => lcr.ClassRoom.ClassName))
                    .Distinct()
                    .ToList();

                return classRooms;
            }
        }
    }
}