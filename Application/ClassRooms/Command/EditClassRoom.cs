using MediatR;
using Persistence;
using AutoMapper;
using FluentValidation;
using Application.Core;

namespace Application.ClassRooms.Command;
public class EditClassRoom
{
    public class Command : IRequest<Result<ClassRoomCreateAndEditDto>>
    {
        public Guid ClassRoomId { get; set; }
        public ClassRoomCreateAndEditDto ClassRoomCreateAndEditDto { get; set; }
    }

    public class CommandValidatorDto : AbstractValidator<Command>
    {
        public CommandValidatorDto()
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
            _mapper = mapper;
            _context = context;
        }

        public async Task<Result<ClassRoomCreateAndEditDto>> Handle(Command request, CancellationToken cancellationToken)
        {
            /** Langkah 1: Mencari Ruang Kelas Berdasarkan ID **/
            var classRoom = await _context.ClassRooms.FindAsync(request.ClassRoomId);

            // Periksa apakah classRoom ditemukan
            /** Langkah 2: Memeriksa Ketersediaan Ruang Kelas **/
            if (classRoom == null)
            {
                return Result<ClassRoomCreateAndEditDto>.Failure("ClassRoom tidak ditemukan");
            }

            /** Langkah 3: Memetakan Data yang Diperbarui dari DTO ke Entitas ClassRoom Menggunakan AutoMapper **/
            _mapper.Map(request.ClassRoomCreateAndEditDto, classRoom);

            /** Langkah 4: Menyimpan Perubahan ke Database **/
            var result = await _context.SaveChangesAsync(cancellationToken) > 0;

            /** Langkah 5: Memeriksa Hasil Simpan **/
            if (!result)
            {
                return Result<ClassRoomCreateAndEditDto>.Failure("Gagal untuk edit ClassRoom");
            }

            /** Langkah 6: Membuat Instance ClassRoomCreateAndEditDto yang Mewakili Hasil Edit **/
            var editedClassRoomCreateAndEditDto = _mapper.Map<ClassRoomCreateAndEditDto>(classRoom);

            /** Langkah 7: Mengembalikan Hasil dalam Bentuk Success Result **/
            return Result<ClassRoomCreateAndEditDto>.Success(editedClassRoomCreateAndEditDto);
        }
    }
}