using Application.Core;
using Domain.ToDoList;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.ToDo.Query;
public class GetAllToDoList
{
    public class Query : IRequest<Result<List<ToDoList>>>
    {
        // Tidak memerlukan parameter tambahan untuk meneruskan ke query
    }

    public class Handler : IRequestHandler<Query, Result<List<ToDoList>>>
    {
        private readonly DataContext _context;
        public Handler(DataContext context)
        {
            _context = context;
        }

        public async Task<Result<List<ToDoList>>> Handle(Query request, CancellationToken cancellationToken)
        {
            return Result<List<ToDoList>>.Success(await _context.ToDoLists
                .OrderByDescending(a => a.CreatedAt)
                .ToListAsync(cancellationToken));
        }
    }
}