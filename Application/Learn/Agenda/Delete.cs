using Application.Core;
using MediatR;
using Persistence;

namespace Application.Learn.Agenda
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
                var schedule = await _context.Schedules.FindAsync(request.Id);

                if (schedule == null) return null;

                _context.Remove(schedule);

                var result = await _context.SaveChangesAsync() > 0;

                if (!result) return Result<Unit>.Failure("Failed to delete Schedule");

                return Result<Unit>.Success(Unit.Value);
            }
        }
    }
}