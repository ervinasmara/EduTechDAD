using Application.Core;
using MediatR;
using Persistence;

namespace Application.ClassRooms
{
    public class Details
    {
        public class Query : IRequest<Result<ClassRoomDto>>
        {
            public Guid Id { get; set; }
        }

        public class Handler : IRequestHandler<Query, Result<ClassRoomDto>>
        {
            private readonly DataContext _context;

            public Handler(DataContext context)
            {
                _context = context;
            }

            public async Task<Result<ClassRoomDto>> Handle(Query request, CancellationToken cancellationToken)
            {
                var classRoom = await _context.ClassRooms.FindAsync(request.Id);

                if (classRoom == null)
                    return Result<ClassRoomDto>.Failure("Attendance not found.");

                var classRoomDto = new ClassRoomDto
                {
                    ClassName = classRoom.ClassName,
                    UniqueNumberOfClassRoom = classRoom.UniqueNumberOfClassRoom,
                };

                return Result<ClassRoomDto>.Success(classRoomDto);
            }
        }
    }
}