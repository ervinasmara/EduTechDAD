using Application.Core;
using FluentValidation;
using MediatR;
using Persistence;

namespace Application.User.Students
{
    public class DeactivateStudent
    {
        public class Command : IRequest<Result<object>>
        {
            public Guid StudentId { get; set; }
        }

        public class Validator : AbstractValidator<Command>
        {
            public Validator()
            {
                RuleFor(x => x.StudentId).NotEmpty();
            }
        }

        public class EditStudentStatusHandler : IRequestHandler<Command, Result<object>>
        {
            private readonly DataContext _context;

            public EditStudentStatusHandler(DataContext context)
            {
                _context = context;
            }

            public async Task<Result<object>> Handle(Command request, CancellationToken cancellationToken)
            {
                var admin = await _context.Students.FindAsync(request.StudentId);

                if (admin == null)
                    return Result<object>.Failure("Student not found");

                // Mengubah status Student menjadi 0
                admin.Status = 0;

                await _context.SaveChangesAsync(cancellationToken);

                return Result<object>.Success(new { Message = "Student status updated successfully" });
            }
        }
    }
}
