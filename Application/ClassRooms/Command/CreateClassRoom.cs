using MediatR;
using Persistence;
using FluentValidation;
using Application.Core;
using AutoMapper;
using Domain.Class;
using Microsoft.EntityFrameworkCore;

namespace Application.ClassRooms.Command
{
    public class CreateClassRoom
    {
        public class Command : IRequest<Result<ClassRoomCreateAndEditDto>>
        {
            public ClassRoomCreateAndEditDto ClassRoomCreateAndEditDto { get; set; }
        }

        public class CommandValidator : AbstractValidator<Command>
        {
            public CommandValidator()
            {
                RuleFor(x => x.ClassRoomCreateAndEditDto).SetValidator(new ClassRoomCreateAndEditValidator());
            }
        }

        public class Handler : IRequestHandler<Command, Result<ClassRoomCreateAndEditDto>>
        {
            private readonly DataContext _context;
            private readonly IMapper _mapper;

            public Handler(DataContext context, IMapper mapper)
            {
                _context = context;
                _mapper = mapper;
            }

            public async Task<Result<ClassRoomCreateAndEditDto>> Handle(Command request, CancellationToken cancellationToken)
            {
                try
                {
                    // Ambil nomor unik terakhir dari database
                    var lastUniqueNumber = await _context.ClassRooms
                        .OrderByDescending(c => c.UniqueNumberOfClassRoom)
                        .Select(c => c.UniqueNumberOfClassRoom)
                        .FirstOrDefaultAsync(cancellationToken);

                    // Jika tidak ada nomor unik sebelumnya, mulai dengan 1
                    int newUniqueNumber = 1;

                    if (!string.IsNullOrEmpty(lastUniqueNumber))
                    {
                        // Ambil angka terakhir dari nomor unik terakhir dan tambahkan satu
                        if (int.TryParse(lastUniqueNumber, out int lastNumber))
                        {
                            newUniqueNumber = lastNumber + 1;
                        }
                        else
                        {
                            // Jika gagal mengonversi, kembalikan pesan kesalahan
                            return Result<ClassRoomCreateAndEditDto>.Failure("Failed to generate UniqueNumberOfClassRoom.");
                        }
                    }

                    // Format nomor unik baru sebagai string dengan panjang 3 digit (contoh: 001, 002, dst.)
                    string newUniqueNumberString = newUniqueNumber.ToString("000");

                    var classRoom = _mapper.Map<ClassRoom>(request.ClassRoomCreateAndEditDto);
                    classRoom.Status = 1;
                    classRoom.UniqueNumberOfClassRoom = newUniqueNumberString;

                    _context.ClassRooms.Add(classRoom);

                    var result = await _context.SaveChangesAsync(cancellationToken) > 0;

                    if (!result)
                        return Result<ClassRoomCreateAndEditDto>.Failure("Failed to Create ClassRoom");

                    var classRoomDto = _mapper.Map<ClassRoomCreateAndEditDto>(classRoom);

                    return Result<ClassRoomCreateAndEditDto>.Success(classRoomDto);
                }
                catch (Exception ex)
                {
                    return Result<ClassRoomCreateAndEditDto>.Failure($"Failed to create ClassRoom: {ex.Message}");
                }
            }
        }
    }
}