using Application.Core;
using Domain.Pengumuman;
using MediatR;
using Persistence;
using System.Diagnostics;

namespace Application.Pengumumans
{
    public class Details
    {
        public class Query : IRequest<Result<Pengumuman>>
        {
            public Guid Id { get; set; }
        }

        public class Handler : IRequestHandler<Query, Result<Pengumuman>>
        {
            private readonly DataContext _context;

            public Handler(DataContext context)
            {
                _context = context;
            }

            public async Task<Result<Pengumuman>> Handle(Query request, CancellationToken cancellationToken)
            {
                var pengumuman = await _context.Pengumumans.FindAsync(request.Id);

                return Result<Pengumuman>.Success(pengumuman);
            }
        }
    }
}