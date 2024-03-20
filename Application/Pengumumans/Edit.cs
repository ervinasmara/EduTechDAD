using AutoMapper;
using Domain.Pengumuman;
using MediatR;
using Persistence;

namespace Application.Pengumumans
{
    public class Edit
    {
        public class Command : IRequest
        {
            public Pengumuman Pengumuman { get; set; }
        }

        public class Handler : IRequestHandler<Command>
        {
            private readonly IMapper _mapper;
            private readonly DataContext _context;

            public Handler(DataContext context, IMapper mapper)
            {
                _mapper = mapper;
                _context = context;
            }

            public async Task Handle(Command request, CancellationToken cancellationToken)
            {
                var pengumuman = await _context.Pengumumans.FindAsync(request.Pengumuman.Id);

                _mapper.Map(request.Pengumuman, pengumuman);

                await _context.SaveChangesAsync();
            }
        }
    }
}