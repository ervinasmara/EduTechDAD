using Domain.Pengumuman;
using MediatR;
using Persistence;

namespace Application.Pengumumans
{
    public class Details
    {
        public class Query : IRequest<Pengumuman>
        {
            public Guid Id { get; set; }
        }

        public class Handler : IRequestHandler<Query, Pengumuman>
        {
            private readonly DataContext _context;

            public Handler(DataContext context)
            {
                _context = context;
            }

            public async Task<Pengumuman> Handle(Query request, CancellationToken cancellationToken)
            {
                return await _context.Pengumumans.FindAsync(request.Id);
            }
        }
    }
}