using Application.Core;
using MediatR;
using Persistence;

namespace Application.ClassRooms.Command
{
    public class DeactiveClassRoom
    {
        public class Command : IRequest<Result<object>>
        {
            public Guid ClassRoomId { get; set; }
        }

        public class Handler : IRequestHandler<Command, Result<object>>
        {
            private readonly DataContext _context;

            public Handler(DataContext context)
            {
                _context = context;
            }

            public async Task<Result<object>> Handle(Command request, CancellationToken cancellationToken)
            {
                var classRoom = await _context.ClassRooms.FindAsync(request.ClassRoomId);

                if (classRoom == null) return null;

                classRoom.Status = 0; // Update status menjadi 0

                var result = await _context.SaveChangesAsync(cancellationToken) > 0;

                if (!result) return Result<object>.Failure("Failed to deactive ClassRoom");

                return Result<object>.Success(new { Message = "ClassRoom status updated successfully" });
            }
        }
    }
}