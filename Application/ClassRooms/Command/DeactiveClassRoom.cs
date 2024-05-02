using Application.Core;
using MediatR;
using Persistence;

namespace Application.ClassRooms.Command
{
    public class DeactiveClassRoom
    {
        public class Command : IRequest<Result<Unit>>
        {
            public Guid ClassRoomId { get; set; }
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
                var classRoom = await _context.ClassRooms.FindAsync(request.ClassRoomId);

                if (classRoom == null) return null;

                classRoom.Status = 2; // Update status menjadi 2

                var result = await _context.SaveChangesAsync(cancellationToken) > 0;

                if (!result) return Result<Unit>.Failure("Failed to deactive ClassRoom");

                return Result<Unit>.Success(Unit.Value);
            }
        }
    }
}