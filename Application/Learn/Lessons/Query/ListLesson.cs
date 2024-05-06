using Application.Core;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Learn.Lessons.Query;
public class ListLesson
{
    public class Query : IRequest<Result<List<LessonGetAllAndByIdDto>>>
    {
        // Tidak memerlukan parameter tambahan
    }

    public class Handler : IRequestHandler<Query, Result<List<LessonGetAllAndByIdDto>>>
    {
        private readonly DataContext _context;
        private readonly IMapper _mapper;

        public Handler(DataContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        // Metode Handle() digunakan untuk menangani permintaan query dengan mencari semua pelajaran beserta nama-nama guru yang mengajarinya.
        public async Task<Result<List<LessonGetAllAndByIdDto>>> Handle(Query request, CancellationToken cancellationToken)
        {
            var lessonsWithClassNameAndTeacherName = await _context.Lessons
                .OrderBy(x => x.Status == 0 ? 1 : 0)
                .ThenByDescending(x => x.Status)
                .ProjectTo<LessonGetAllAndByIdDto>(_mapper.ConfigurationProvider)
                .ToListAsync(cancellationToken);

            return Result<List<LessonGetAllAndByIdDto>>.Success(lessonsWithClassNameAndTeacherName);
        }
    }
}