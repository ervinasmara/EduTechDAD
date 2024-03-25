using Application.Core;
using Domain.Class;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.ClassRooms
{
    public class List
    {
        public class Query : IRequest<Result<List<ClassRoom>>> // Dan ini akan berasal dari atau menggunakan IRequest dari Mediator
        {
            /* Kita tidak memerlukan parameter tambahan untuk meneruskan ke query*/
        }

        public class Handler : IRequestHandler<Query, Result<List<ClassRoom>>>
        {
            private readonly DataContext _context;
            public Handler(DataContext context) // Memanggil DataContext dan menyebutnya sebagai context
            {
                _context = context;
            }
            public async Task<Result<List<ClassRoom>>> Handle(Query request, CancellationToken cancellationToken)
            {
                return Result<List<ClassRoom>>.Success(await _context.ClassRooms.ToListAsync(cancellationToken));
            }
        }
    }
}