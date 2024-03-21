using Application.Core;
using Domain.Announcement;
using MediatR;
using Persistence;

namespace Application.Announcements
{
    public class Details
    {
        public class Query : IRequest<Result<Announcement>>
        {
            public Guid Id { get; set; }
        }

        public class Handler : IRequestHandler<Query, Result<Announcement>>
        {
            private readonly DataContext _context;

            public Handler(DataContext context)
            {
                _context = context;
            }

            public async Task<Result<Announcement>> Handle(Query request, CancellationToken cancellationToken)
            {
                var announcement = await _context.Announcements.FindAsync(request.Id);

                return Result<Announcement>.Success(announcement);
            }
        }
    }
}