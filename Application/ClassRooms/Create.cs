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
                var classRoom = new ClassRoom
                {
                    ClassName = request.ClassRoomDto.ClassName,
                    UniqueNumber = request.ClassRoomDto.UniqueNumber,
                };

                _context.ClassRooms.Add(classRoom);

                var result = await _context.SaveChangesAsync(cancellationToken) > 0;

                if (!result) return Result<ClassRoomDto>.Failure("Gagal Untuk Membuat ClassRoom");

                // Buat DTO dari classRoom yang telah dibuat
                var classRoomDto = _mapper.Map<ClassRoomDto>(classRoom);

                return Result<ClassRoomDto>.Success(classRoomDto);
            }
        }
    }
}
