using Application.Core;
using MediatR;
using Persistence;

namespace Application.ClassRooms
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
                var classRoom = await _context.ClassRooms.FindAsync(request.Id);

                if (classRoom == null) return null;

                _context.Remove(classRoom);

                var result = await _context.SaveChangesAsync() > 0;

                if (!result) return Result<Unit>.Failure("Gagal untuk menghapus ClassRoom");

                return Result<Unit>.Success(Unit.Value);
            }
        }
    }
}