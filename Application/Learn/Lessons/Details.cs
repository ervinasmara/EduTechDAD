using Application.Core;
using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Learn.Lessons
{
    public class Details
    {
        public class Query : IRequest<Result<LessonGetAllDto>>
        {
            public Guid Id { get; set; }
        }

        public class Handler : IRequestHandler<Query, Result<LessonGetAllDto>>
        {
            private readonly DataContext _context;
            private readonly IMapper _mapper;

            public Handler(DataContext context, IMapper mapper)
            {
                _context = context;
                _mapper = mapper;
            }

            public async Task<Result<LessonGetAllDto>> Handle(Query request, CancellationToken cancellationToken)
            {
                var lesson = await _context.Lessons
                    .Include(l => l.TeacherLessons)
                    .ThenInclude(tl => tl.Teacher)
                    .FirstOrDefaultAsync(l => l.Id == request.Id);

                if (lesson == null)
                    return Result<LessonGetAllDto>.Failure($"Lesson with id {request.Id} not found.");

                var lessonDto = _mapper.Map<LessonGetAllDto>(lesson);
                return Result<LessonGetAllDto>.Success(lessonDto);
            }
        }
    }
}