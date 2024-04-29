using Application.Core;
using MediatR;
using Persistence;

namespace Application.Learn.Courses.Command
{
    public class DeleteCourse
    {
        public class Command : IRequest<Result<Unit>>
        {
            public Guid CourseId { get; set; }
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
                var course = await _context.Courses.FindAsync(request.CourseId);

                if (course == null) return null;

                _context.Remove(course);

                var result = await _context.SaveChangesAsync() > 0;

                if (!result) return Result<Unit>.Failure("Failed to delete Course");

                return Result<Unit>.Success(Unit.Value);
            }
        }
    }
}