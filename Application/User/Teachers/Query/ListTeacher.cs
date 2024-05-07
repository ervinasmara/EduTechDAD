using Application.Core;
using Application.User.DTOs;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.User.Teachers.Query;
public class ListTeacher
{
    public class Query : IRequest<Result<List<TeacherGetAllAndByIdDto>>>
    {
        // Tidak memerlukan parameter tambahan untuk meneruskan ke query
    }

    public class Handler : IRequestHandler<Query, Result<List<TeacherGetAllAndByIdDto>>>
    {
        private readonly DataContext _context;
        private readonly IMapper _mapper;

        public Handler(DataContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<Result<List<TeacherGetAllAndByIdDto>>> Handle(Query request, CancellationToken cancellationToken)
        {
            /** Langkah 1: Membuat query untuk mendapatkan semua guru yang aktif **/
            var teacherQuery = _context.Teachers
                .Where(s => s.Status != 0) // Hanya guru dengan status aktif (tidak dinonaktifkan)
                .OrderBy(a => a.NameTeacher) // Urutkan berdasarkan nama guru
                .ProjectTo<TeacherGetAllAndByIdDto>(_mapper.ConfigurationProvider); // Menggunakan ProjectTo untuk memetakan ke DTO

            /** Langkah 2: Eksekusi query dan ambil hasil **/
            var teacherDtos = await teacherQuery.ToListAsync(cancellationToken);

            /** Langkah 3: Memeriksa apakah ada guru yang ditemukan **/
            if (!teacherDtos.Any())
            {
                return Result<List<TeacherGetAllAndByIdDto>>.Failure("Tidak ada guru yang ditemukan");
            }

            /** Langkah 4: Mengembalikan hasil dalam bentuk Success Result dengan daftar guru yang ditemukan **/
            return Result<List<TeacherGetAllAndByIdDto>>.Success(teacherDtos);
        }
    }
}