using Application.Core;
using Domain.Learn.Subject;
using MediatR;
using Persistence;

namespace Application.Learn.Subject
{
    public class Details
    {
        public class Query : IRequest<Result<Lesson>>
        {
            public Guid Id { get; set; }
        }

        public class Handler : IRequestHandler<Query, Result<Lesson>>
        {
            private readonly DataContext _context;

            public Handler(DataContext context)
            {
                _context = context;
            }

            public async Task<Result<Lesson>> Handle(Query request, CancellationToken cancellationToken)
            {
                var lesson = await _context.Lessons.FindAsync(request.Id);

                return Result<Lesson>.Success(lesson);
            }
        }
    }
}