using Application.Core;
using MediatR;
using Persistence;

namespace Application.Learn.Lessons.Command;
public class DeleteLesson
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
            var lesson = await _context.Lessons.FindAsync(request.Id);

            if (lesson == null) return null;

            _context.Remove(lesson);

            var result = await _context.SaveChangesAsync() > 0;

            if (!result) return Result<Unit>.Failure("Gagal untuk menghapus pelajaran");

            return Result<Unit>.Success(Unit.Value);
        }
    }
}