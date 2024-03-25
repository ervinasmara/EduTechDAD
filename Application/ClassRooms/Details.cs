using Application.Core;
using Domain.Class;
using MediatR;
using Persistence;

namespace Application.ClassRooms
{
    public class Details
    {
        public class Query : IRequest<Result<ClassRoom>>
        {
            public Guid Id { get; set; }
        }

        public class Handler : IRequestHandler<Query, Result<ClassRoom>>
        {
            private readonly DataContext _context;

            public Handler(DataContext context)
            {
                _context = context;
            }

            public async Task<Result<ClassRoom>> Handle(Query request, CancellationToken cancellationToken)
            {
                var classRoom = await _context.ClassRooms.FindAsync(request.Id);

                return Result<ClassRoom>.Success(classRoom);
            }
        }
    }
}