using Application.Core;
using Application.User.DTOs;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.User.Students.Query;
public class DetailsStudent
{
    public class Query : IRequest<Result<StudentGetByIdDto>>
    {
        public Guid StudentId { get; set; }
    }

    public class Handler : IRequestHandler<Query, Result<StudentGetByIdDto>>
    {
        private readonly DataContext _context;
        private readonly IMapper _mapper;

        public Handler(DataContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<Result<StudentGetByIdDto>> Handle(Query request, CancellationToken cancellationToken)
        {
            /** Langkah 1: Mencari siswa berdasarkan ID **/
            var student = await _context.Students
                .Include(s => s.User) // Memuat relasi User
                .Include(s => s.ClassRoom) // Memuat relasi ClassRoom
                .Where(s => s.Id == request.StudentId) // Memfilter berdasarkan StudentId
                .ProjectTo<StudentGetByIdDto>(_mapper.ConfigurationProvider) // Memetakan hasil ke DTO menggunakan AutoMapper
                .FirstOrDefaultAsync(cancellationToken); // Ambil siswa pertama yang cocok dengan ID yang diberikan

            /** Langkah 2: Memeriksa apakah siswa ditemukan **/
            if (student == null)
            {
                return Result<StudentGetByIdDto>.Failure("Siswa tidak ditemukan");
            }

            /** Langkah 3: Mengembalikan hasil dalam bentuk Success Result dengan data siswa yang ditemukan **/
            return Result<StudentGetByIdDto>.Success(student);
        }
    }
}