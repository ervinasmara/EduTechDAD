using Application.Core;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Learn.Lessons.Query;
public class DetailsLesson
{
    public class Query : IRequest<Result<LessonGetAllAndByIdDto>>
    {
        public Guid LessonId { get; set; } // ID pelajaran yang ingin diambil
    }

    public class Handler : IRequestHandler<Query, Result<LessonGetAllAndByIdDto>>
    {
        private readonly DataContext _context;
        private readonly IMapper _mapper;

        public Handler(DataContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<Result<LessonGetAllAndByIdDto>> Handle(Query request, CancellationToken cancellationToken)
        {
            var lessonsWithClassNameAndTeacherName = await _context.Lessons
                .Where(l => l.Id == request.LessonId)
                .ProjectTo<LessonGetAllAndByIdDto>(_mapper.ConfigurationProvider)
                .FirstOrDefaultAsync(cancellationToken);

            return Result<LessonGetAllAndByIdDto>.Success(lessonsWithClassNameAndTeacherName);
        }
    }
}