using Application.Core;
using Domain.Present;
using MediatR;
using Persistence;

namespace Application.Presents
{
    public class Details
    {
        public class Query : IRequest<Result<Attendance>>
        {
            public Guid Id { get; set; }
        }

        public class Handler : IRequestHandler<Query, Result<Attendance>>
        {
            private readonly DataContext _context;

            public Handler(DataContext context)
            {
                _context = context;
            }

            public async Task<Result<Attendance>> Handle(Query request, CancellationToken cancellationToken)
            {
                var attendance = await _context.Attendances.FindAsync(request.Id);

                return Result<Attendance>.Success(attendance);
            }
        }
    }
}