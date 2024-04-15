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
                var lessonsWithTeacherName = await _context.Lessons
                    .Select(lesson => new LessonGetAllDto
                    {
                        Id = lesson.Id,
                        LessonName = lesson.LessonName,
                        UniqueNumberOfLesson = lesson.UniqueNumberOfLesson,
                        TeacherId = lesson.TeacherId,
                        NameTeacher = _context.Teachers.FirstOrDefault(t => t.Id == lesson.TeacherId).NameTeacher ?? "Unknown"
                    })
                    .ToListAsync(cancellationToken);

                return Result<List<LessonGetAllDto>>.Success(lessonsWithTeacherName);
            }
        }
    }
}