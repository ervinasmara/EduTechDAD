using Application.Core;
using Domain.ToDoList;
using MediatR;
using Persistence;

namespace Application.ToDo.Query;
public class DetailsToDoList
{
    public class Query : IRequest<Result<ToDoList>>
    {
        public Guid ToDoListId { get; set; }
    }

    public class Handler : IRequestHandler<Query, Result<ToDoList>>
    {
        private readonly DataContext _context;

        public Handler(DataContext context)
        {
            _context = context;
        }

        public async Task<Result<ToDoList>> Handle(Query request, CancellationToken cancellationToken)
        {
            var toDoList = await _context.ToDoLists.FindAsync(request.ToDoListId);

            return Result<ToDoList>.Success(toDoList);
        }
    }
}