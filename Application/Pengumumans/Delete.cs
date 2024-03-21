using Application.Core;
using MediatR;
using Persistence;

namespace Application.Pengumumans
{
    public class Delete
    {
        public class Command : IRequest<Result<Unit>>
        {
            public Guid Id { get; set; }
        }

        public class Handler : IRequestHandler<Command, Result<Unit>>
        {
            private readonly DataContext _context;

            public Handler(DataContext context)
            {
                _context = context;
            }

            public async Task<Result<Unit>> Handle(Command request, CancellationToken cancellationToken)
            {
                var pengumuman = await _context.Pengumumans.FindAsync(request.Id);

                if (pengumuman == null) return null;

                _context.Remove(pengumuman);

                var result = await _context.SaveChangesAsync() > 0;

                if (!result) return Result<Unit>.Failure("Gagal untuk menghapus Pengumuman");

                return Result<Unit>.Success(Unit.Value);
            }
        }
    }
}