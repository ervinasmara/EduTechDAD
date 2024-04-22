using Application.Core;
using MediatR;
using Persistence;

namespace Application.Assignments
{
    public class DeleteTask
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
                var assignment = await _context.Assignments.FindAsync(request.Id);

                if (assignment == null) return null;

                _context.Remove(assignment);

                var result = await _context.SaveChangesAsync() > 0;

                if (!result) return Result<Unit>.Failure("Failed to delete Assignment");

                return Result<Unit>.Success(Unit.Value);
            }
        }
    }
}