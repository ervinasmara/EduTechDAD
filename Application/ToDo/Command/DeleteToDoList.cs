using Application.Core;
using MediatR;
using Persistence;

namespace Application.ToDo.Command;
public class DeleteToDoList
{
    public class Command : IRequest<Result<Unit>>
    {
        public Guid ToDoListId { get; set; }
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
            var toDoList = await _context.ToDoLists.FindAsync(request.ToDoListId);

            if (toDoList == null) return null;

            _context.Remove(toDoList);

            var result = await _context.SaveChangesAsync() > 0;

            if (!result) return Result<Unit>.Failure("Failed to delete ToDoList");

            return Result<Unit>.Success(Unit.Value);
        }
    }
}