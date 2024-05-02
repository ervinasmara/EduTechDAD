using Application.Core;
using AutoMapper;
using MediatR;
using Persistence;

namespace Application.ClassRooms.Query
{
    public class DetailsClassRoom
    {
        public class Query : IRequest<Result<ClassRoomGetDto>>
        {
            public Guid ClassRoomId { get; set; }
        }

        public class Handler : IRequestHandler<Query, Result<ClassRoomGetDto>>
        {
            private readonly DataContext _context;
            private readonly IMapper _mapper;

            public Handler(DataContext context, IMapper mapper)
            {
                _context = context;
                _mapper = mapper;
            }

            public async Task<Result<ClassRoomGetDto>> Handle(Query request, CancellationToken cancellationToken)
            {
                var classRoom = await _context.ClassRooms.FindAsync(request.ClassRoomId);

                if (classRoom == null)
                    return Result<ClassRoomGetDto>.Failure("ClassRoom not found.");

                var classRoomDtos = _mapper.Map<ClassRoomGetDto>(classRoom);

                return Result<ClassRoomGetDto>.Success(classRoomDtos);
            }
        }
    }
}