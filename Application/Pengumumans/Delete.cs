using MediatR; // Mengunakan Fungsi "IRequest"
using Persistence; // Memanggil class "DataContext"

namespace Application.Pengumumans
{
    public class Delete
    {
        public class Command : IRequest
        {
            public Guid Id { get; set; }
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
                var pengumuman = await _context.Pengumumans.FindAsync(request.Id);

                _context.Remove(pengumuman);

                await _context.SaveChangesAsync();
            }
        }
    }
}