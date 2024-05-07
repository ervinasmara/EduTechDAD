using MediatR;
using Persistence;
using FluentValidation;
using Application.Core;
using AutoMapper;
using Domain.Class;
using Microsoft.EntityFrameworkCore;

namespace Application.ClassRooms.Command;
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
                /** Langkah 1: Ambil nomor unik terakhir dari database **/
                var lastUniqueNumber = await _context.ClassRooms
                    .OrderByDescending(c => c.UniqueNumberOfClassRoom)
                    .Select(c => c.UniqueNumberOfClassRoom)
                    .FirstOrDefaultAsync(cancellationToken);

                /** Langkah 2: Inisialisasi nomor unik baru **/
                int newUniqueNumber = 1;

                /** Langkah 3: Tentukan nomor unik baru berdasarkan nomor unik terakhir **/
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
                        return Result<ClassRoomCreateAndEditDto>.Failure("Gagal menghasilkan UniqueNumberOfClassRoom.");
                    }
                }

                /** Langkah 4: Format nomor unik baru sebagai string dengan panjang 3 digit **/
                string newUniqueNumberString = newUniqueNumber.ToString("000");

                /** Langkah 5: Memetakan data dari DTO ke entitas ClassRoom menggunakan AutoMapper **/
                var classRoom = _mapper.Map<ClassRoom>(request.ClassRoomCreateAndEditDto);
                classRoom.Status = 1;
                classRoom.UniqueNumberOfClassRoom = newUniqueNumberString;

                /** Langkah 6: Menambahkan entitas ClassRoom ke database **/
                _context.ClassRooms.Add(classRoom);

                /** Langkah 7: Menyimpan perubahan ke database **/
                var result = await _context.SaveChangesAsync(cancellationToken) > 0;

                /** Langkah 8: Memeriksa hasil penyimpanan **/
                if (!result)
                    return Result<ClassRoomCreateAndEditDto>.Failure("Gagal untuk membuat ClassRoom");

                /** Langkah 9: Memetakan kembali entitas ClassRoom ke DTO **/
                var classRoomDto = _mapper.Map<ClassRoomCreateAndEditDto>(classRoom);

                /** Langkah 10: Mengembalikan hasil dalam bentuk Success Result **/
                return Result<ClassRoomCreateAndEditDto>.Success(classRoomDto);
            }
            catch (Exception ex)
            {
                /** Langkah 11: Menangani kesalahan jika terjadi **/
                return Result<ClassRoomCreateAndEditDto>.Failure($"Gagal untuk membuat ClassRoom: {ex.Message}");
            }
        }
    }
}