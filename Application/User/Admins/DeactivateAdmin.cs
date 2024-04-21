using Application.Core;
using FluentValidation;
using MediatR;
using Persistence;

namespace Application.User.Admins
{
    public class DeactivateAdmin
    {
        public class Command : IRequest<Result<object>>
        {
            public Guid AdminId { get; set; }
        }

        public class Validator : AbstractValidator<Command>
        {
            public Validator()
            {
                RuleFor(x => x.AdminId).NotEmpty();
            }
        }

        public class EditAdminStatusHandler : IRequestHandler<Command, Result<object>>
        {
            private readonly DataContext _context;

            public EditAdminStatusHandler(DataContext context)
            {
                _context = context;
            }

            public async Task<Result<object>> Handle(Command request, CancellationToken cancellationToken)
            {
                var admin = await _context.Admins.FindAsync(request.AdminId);

                if (admin == null)
                    return Result<object>.Failure("Admin not found");

                // Mengubah status Admin menjadi 0
                admin.Status = 0;

                await _context.SaveChangesAsync(cancellationToken);

                return Result<object>.Success(new { Message = "Admin status updated successfully" });
            }
        }
    }
}
