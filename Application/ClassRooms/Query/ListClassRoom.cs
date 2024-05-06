using Application.Core;
using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.ClassRooms.Query;
public class ListClassRoom
{
    public class Query : IRequest<Result<List<ClassRoomGetDto>>>
    {
        /* Kita tidak memerlukan parameter tambahan untuk meneruskan ke query*/
    }

    public class Handler : IRequestHandler<Query, Result<List<ClassRoomGetDto>>>
    {
        private readonly DataContext _context;
        private readonly IMapper _mapper;

        public Handler(DataContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<Result<List<ClassRoomGetDto>>> Handle(Query request, CancellationToken cancellationToken)
        {
            /** Langkah 1: Mengambil Semua Data Ruang Kelas dari Database **/
            var classRoom = await _context.ClassRooms
                .ToListAsync(cancellationToken);

            /** Langkah 2: Memetakan Data Ruang Kelas ke ClassRoomGetDto Menggunakan AutoMapper **/
            var classRoomDtos = _mapper.Map<List<ClassRoomGetDto>>(classRoom);

            /** Langkah 3: Mengembalikan Hasil dalam Bentuk Success Result **/
            return Result<List<ClassRoomGetDto>>.Success(classRoomDtos);
        }
    }
}