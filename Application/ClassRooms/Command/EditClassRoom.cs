using MediatR;
using Persistence;
using AutoMapper;
using FluentValidation;
using Application.Core;

namespace Application.ClassRooms.Command
{
    public class EditClassRoom
    {
        public class Command : IRequest<Result<ClassRoomCreateAndEditDto>>
        {
            public Guid Id { get; set; }
            public ClassRoomCreateAndEditDto ClassRoomCreateAndEditDto { get; set; }
        }

        public class CommandValidatorDto : AbstractValidator<Command>
        {
            public CommandValidatorDto()
            {
                RuleFor(x => x.ClassRoomCreateAndEditDto).SetValidator(new ClassRoomValidator());
            }
        }

        public class Handler : IRequestHandler<Command, Result<ClassRoomCreateAndEditDto>>
        {
            private readonly DataContext _context;
            private readonly IMapper _mapper;

            public Handler(DataContext context, IMapper mapper)
            {
                _mapper = mapper;
                _context = context;
            }

            public async Task<Result<ClassRoomCreateAndEditDto>> Handle(Command request, CancellationToken cancellationToken)
            {
                var classRoom = await _context.ClassRooms.FindAsync(request.Id);

                // Periksa apakah classRoom ditemukan
                if (classRoom == null)
                {
                    return Result<ClassRoomCreateAndEditDto>.Failure("ClassRoom Not Found");
                }

                _mapper.Map(request.ClassRoomCreateAndEditDto, classRoom);

                var result = await _context.SaveChangesAsync(cancellationToken) > 0;

                if (!result)
                {
                    return Result<ClassRoomCreateAndEditDto>.Failure("Failed to edit ClassRoom");
                }

                // Buat instance ClassRoomCreateAndEditDto yang mewakili hasil edit
                var editedClassRoomCreateAndEditDto = _mapper.Map<ClassRoomCreateAndEditDto>(classRoom);

                return Result<ClassRoomCreateAndEditDto>.Success(editedClassRoomCreateAndEditDto);
            }
        }
    }
}
