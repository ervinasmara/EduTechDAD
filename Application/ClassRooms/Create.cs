using MediatR;
using Persistence;
using FluentValidation;
using Application.Core;
using AutoMapper;
using Domain.Class;

namespace Application.ClassRooms
{
    public class Create
    {
        public class Command : IRequest<Result<ClassRoomDto>>
        {
            public ClassRoomDto ClassRoomDto { get; set; }
        }

        public class CommandValidator : AbstractValidator<Command>
        {
            public CommandValidator()
            {
                RuleFor(x => x.ClassRoomDto).SetValidator(new ClassRoomValidator());
            }
        }

        public class Handler : IRequestHandler<Command, Result<ClassRoomDto>>
        {
            private readonly DataContext _context;
            private readonly IMapper _mapper;

            public Handler(DataContext context, IMapper mapper)
            {
                _context = context;
                _mapper = mapper;
            }

            public async Task<Result<ClassRoomDto>> Handle(Command request, CancellationToken cancellationToken)
            {
                var uniqueNumber = request.ClassRoomDto.UniqueNumberOfClassRoom;

                // Cek apakah UniqueNumberOfClassRoom sudah ada di database
                var isUnique = !_context.ClassRooms.Any(x => x.UniqueNumberOfClassRoom == uniqueNumber);

                if (!isUnique)
                {
                    return Result<ClassRoomDto>.Failure("UniqueNumberOfClassRoom already exists");
                }

                var classRoom = new ClassRoom
                {
                    ClassName = request.ClassRoomDto.ClassName,
                    UniqueNumberOfClassRoom = uniqueNumber,
                };

                _context.ClassRooms.Add(classRoom);

                var result = await _context.SaveChangesAsync(cancellationToken) > 0;

                if (!result)
                    return Result<ClassRoomDto>.Failure("Failed to Create ClassRoom");

                var classRoomDto = _mapper.Map<ClassRoomDto>(classRoom);

                return Result<ClassRoomDto>.Success(classRoomDto);
            }
        }
    }
}