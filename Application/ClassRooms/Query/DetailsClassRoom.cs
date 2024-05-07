using Application.Core;
using AutoMapper;
using MediatR;
using Persistence;

namespace Application.ClassRooms.Query;
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
            /** Langkah 1: Mencari Ruang Kelas Berdasarkan ID **/
            var classRoom = await _context.ClassRooms.FindAsync(request.ClassRoomId);

            /** Langkah 2: Memeriksa Ketersediaan Ruang Kelas **/
            if (classRoom == null)
                return Result<ClassRoomGetDto>.Failure("ClassRoom tidak ditemukan");

            /** Langkah 3: Memetakan Data Ruang Kelas ke ClassRoomGetDto Menggunakan AutoMapper **/
            var classRoomDtos = _mapper.Map<ClassRoomGetDto>(classRoom);

            /** Langkah 4: Mengembalikan Hasil dalam Bentuk Success Result **/
            return Result<ClassRoomGetDto>.Success(classRoomDtos);
        }
    }
}