using Application.Core;
using FluentValidation;
using MediatR;
using Persistence;

namespace Application.User.Teachers.Command
{
    public class DeactivateTeacher
    {
        public class Command : IRequest<Result<object>>
        {
            public Guid TeacherId { get; set; }
        }

        public class Validator : AbstractValidator<Command>
        {
            public Validator()
            {
                RuleFor(x => x.TeacherId).NotEmpty();
            }
        }

        public class EditTeacherStatusHandler : IRequestHandler<Command, Result<object>>
        {
            private readonly DataContext _context;

            public EditTeacherStatusHandler(DataContext context)
            {
                _context = context;
            }

            public async Task<Result<object>> Handle(Command request, CancellationToken cancellationToken)
            {
                var admin = await _context.Teachers.FindAsync(request.TeacherId);

                if (admin == null)
                    return Result<object>.Failure("Teacher not found");

                // Mengubah status Teacher menjadi 0
                admin.Status = 0;

                await _context.SaveChangesAsync(cancellationToken);

                return Result<object>.Success(new { Message = "Teacher status updated successfully" });
            }
        }
    }
}
