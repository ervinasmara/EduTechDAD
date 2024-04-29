using Application.Core;
using MediatR;
using Persistence;

namespace Application.ClassRooms.Query
{
    public class Details
    {
        public class Query : IRequest<Result<ClassRoomGetDto>>
        {
            public Guid Id { get; set; }
        }

        public class Handler : IRequestHandler<Query, Result<ClassRoomGetDto>>
        {
            private readonly DataContext _context;

            public Handler(DataContext context)
            {
                _context = context;
            }

            public async Task<Result<ClassRoomGetDto>> Handle(Query request, CancellationToken cancellationToken)
            {
                var classRoom = await _context.ClassRooms.FindAsync(request.Id);

                if (classRoom == null)
                    return Result<ClassRoomGetDto>.Failure("Attendance not found.");

                var classRoomDto = new ClassRoomGetDto
                {
                    ClassName = classRoom.ClassName,
                    LongClassName = classRoom.LongClassName,
                    UniqueNumberOfClassRoom = classRoom.UniqueNumberOfClassRoom,
                };

                return Result<ClassRoomGetDto>.Success(classRoomDto);
            }
        }
    }
}