using Application.Core;
using Application.User.DTOs;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.User.Students.Query;
public class ListStudent
{
    public class Query : IRequest<Result<List<StudentGetAllDto>>>
    {
        /* Kita tidak memerlukan parameter tambahan untuk meneruskan ke query*/
    }

    public class Handler : IRequestHandler<Query, Result<List<StudentGetAllDto>>>
    {
        private readonly DataContext _context;
        private readonly IMapper _mapper;

        public Handler(DataContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<Result<List<StudentGetAllDto>>> Handle(Query request, CancellationToken cancellationToken)
        {
            /** Langkah 1: Mengambil daftar siswa dari database **/
            var students = await _context.Students
                .Where(s => s.Status != 0) // Filter status siswa yang tidak sama dengan 0 (status aktif)
                .OrderBy(s => s.Nis) // Mengurutkan siswa berdasarkan NIS secara naik
                .ProjectTo<StudentGetAllDto>(_mapper.ConfigurationProvider) // Memetakan hasil ke DTO menggunakan AutoMapper
                .ToListAsync(cancellationToken); // Mengubah hasil ke daftar dan mengirimkannya ke dalam daftar

            /** Langkah 2: Mengembalikan daftar siswa dalam bentuk Success Result **/
            return Result<List<StudentGetAllDto>>.Success(students);
        }
    }
}