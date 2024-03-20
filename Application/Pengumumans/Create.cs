using Domain.Pengumuman;
using MediatR;
using Persistence;

namespace Application.Pengumumans
{
    public class Create
    {
        public class Command : IRequest
        {
            public Pengumuman Pengumuman { get; set; }
        }

        public class Handler : IRequestHandler<Command>
        {
            private readonly DataContext _context;

            public Handler(DataContext context)
            {
                _context = context;
            }

            public async Task Handle(Command request, CancellationToken cancellationToken)
            {
                _context.Pengumumans.Add(request.Pengumuman);

                await _context.SaveChangesAsync();
            }
        }
    }
}