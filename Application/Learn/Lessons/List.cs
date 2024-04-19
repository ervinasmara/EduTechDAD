using Application.Core;
using Application.User.DTOs;
using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Learn.Lessons
{
    public class List
    {
        public class Query : IRequest<Result<List<LessonGetAllDto>>>
        {
            // Tidak memerlukan parameter tambahan
        }

        public class Handler : IRequestHandler<Query, Result<List<LessonGetAllDto>>>
        {
            private readonly DataContext _context;
            private readonly IMapper _mapper;

            public Handler(DataContext context, IMapper mapper)
            {
                _context = context;
                _mapper = mapper;
            }

            public async Task<Result<List<LessonGetAllDto>>> Handle(Query request, CancellationToken cancellationToken)
            {
                var lessonsWithTeacherNames = await _context.Lessons
                    .Select(lesson => new LessonGetAllDto
                    {
                        Id = lesson.Id,
                        LessonName = lesson.LessonName,
                        UniqueNumberOfLesson = lesson.UniqueNumberOfLesson,
                        ClassNames = _context.LessonClassRooms
                            .Where(lcr => lcr.LessonId == lesson.Id)
                            .Select(lcr => lcr.ClassRoom.ClassName)
                            .ToList(),
                        TeacherLesson = _context.TeacherLessons
                            .Where(tl => tl.LessonId == lesson.Id)
                            .Select(tl => new TeacherLessonGetAllDto
                            {
                                NameTeacher = tl.Teacher.NameTeacher,
                                ClassNames = _context.TeacherClassRooms
                                    .Where(tc => tc.TeacherId == tl.TeacherId)
                                    .Select(tc => tc.ClassRoom.ClassName)
                                    .ToList()
                            })
                            .ToList()
                    })
                    .ToListAsync(cancellationToken);

                return Result<List<LessonGetAllDto>>.Success(lessonsWithTeacherNames);
            }
        }
    }
}