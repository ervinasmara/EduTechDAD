using Application.Core;
using Application.User.DTOs;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.User.Teachers.Query;
public class DetailsTeacher
{
    public class Query : IRequest<Result<TeacherGetAllAndByIdDto>>
    {
        public Guid TeacherId { get; set; }
    }

    public class Handler : IRequestHandler<Query, Result<TeacherGetAllAndByIdDto>>
    {
        private readonly DataContext _context;
        private readonly IMapper _mapper;

        public Handler(DataContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<Result<TeacherGetAllAndByIdDto>> Handle(Query request, CancellationToken cancellationToken)
        {
            /** Langkah 1: Mencari guru berdasarkan ID **/
            var teacher = await _context.Teachers
                .ProjectTo<TeacherGetAllAndByIdDto>(_mapper.ConfigurationProvider) // Memetakan hasil ke DTO menggunakan AutoMapper
                .FirstOrDefaultAsync(t => t.Id == request.TeacherId); // Ambil guru pertama dengan ID yang cocok dengan permintaan

            /** Langkah 2: Memeriksa apakah guru ditemukan **/
            if (teacher == null)
                return Result<TeacherGetAllAndByIdDto>.Failure("Guru tidak ditemukan");

            /** Langkah 3: Mengembalikan hasil dalam bentuk Success Result dengan data guru yang ditemukan **/
            return Result<TeacherGetAllAndByIdDto>.Success(teacher);
        }
    }
}