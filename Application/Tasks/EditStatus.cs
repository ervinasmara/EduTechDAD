using Application.Core;
using FluentValidation;
using MediatR;
using Persistence;

namespace Application.Tasks
{
    public class EditStatus
    {
        public class Command : IRequest<Result<Unit>>
        {
            public Guid Id { get; set; }
        }

        public class CommandValidator : AbstractValidator<Command>
        {
            public CommandValidator()
            {
                RuleFor(x => x.Id).NotEmpty();
            }
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

                if (assignment == null)
                    return Result<Unit>.Failure("Assignment not found");

                assignment.Status = 2;

                var result = await _context.SaveChangesAsync(cancellationToken) > 0;

                if (!result)
                    return Result<Unit>.Failure("Failed to update Assignment status");

                return Result<Unit>.Success(Unit.Value);
            }
        }
    }
}