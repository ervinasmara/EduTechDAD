using MediatR;
using Persistence;
using AutoMapper;
using FluentValidation;
using Application.Core;
using Domain.Class;

namespace Application.ClassRooms
{
    public class Edit
    {
        public class Command : IRequest<Result<ClassRoomDto>>
        {
            public Guid Id { get; set; }
            public ClassRoomDto ClassRoomDto { get; set; }
        }

        public class CommandValidatorDto : AbstractValidator<Command>
        {
            public CommandValidatorDto()
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
                _mapper = mapper;
                _context = context;
            }

            public async Task<Result<ClassRoomDto>> Handle(Command request, CancellationToken cancellationToken)
            {
                var classRoom = await _context.ClassRooms.FindAsync(request.Id);

                // Periksa apakah classRoom ditemukan
                if (classRoom == null)
                {
                    return Result<ClassRoomDto>.Failure("ClassRoom Not Found");
                }

                // Cek apakah UniqueNumberOfClassRoom sudah ada di database
                var uniqueNumber = request.ClassRoomDto.UniqueNumberOfClassRoom;
                var isUnique = !_context.ClassRooms.Any(x => x.UniqueNumberOfClassRoom == uniqueNumber && x.Id != request.Id);

                if (!isUnique)
                {
                    return Result<ClassRoomDto>.Failure("UniqueNumberOfClassRoom already exists");
                }

                _mapper.Map(request.ClassRoomDto, classRoom);

                var result = await _context.SaveChangesAsync(cancellationToken) > 0;

                if (!result)
                {
                    return Result<ClassRoomDto>.Failure("Failed to edit ClassRoom");
                }

                // Buat instance ClassRoomDto yang mewakili hasil edit
                var editedClassRoomDto = _mapper.Map<ClassRoomDto>(classRoom);

                return Result<ClassRoomDto>.Success(editedClassRoomDto);
            }
        }
    }
}
