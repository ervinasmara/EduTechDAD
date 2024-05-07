using Application.Core;
using Application.Interface;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Learn.Courses.Query;
public class ListCourseByTeacherId
{
    public class Query : IRequest<Result<List<CourseGetDto>>>
    {
    }

    public class Handler : IRequestHandler<Query, Result<List<CourseGetDto>>>
    {
        private readonly DataContext _context;
        private readonly IMapper _mapper;
        private readonly IUserAccessor _userAccessor;

        public Handler(DataContext context, IMapper mapper, IUserAccessor userAccessor)
        {
            _context = context;
            _mapper = mapper;
            _userAccessor = userAccessor;
        }

        public async Task<Result<List<CourseGetDto>>> Handle(Query request, CancellationToken cancellationToken)
        {
            /** Langkah 1: Dapatkan TeacherId dari token **/
            var teacherIdString = _userAccessor.GetTeacherIdFromToken();

            /** Langkah 2: Periksa jika TeacherId ada di token **/
            if (string.IsNullOrEmpty(teacherIdString))
            {
                return Result<List<CourseGetDto>>.Failure("TeacherId tidak ditemukan ditoken");
            }

            if (!Guid.TryParse(teacherIdString, out var teacherId))
            {
                return Result<List<CourseGetDto>>.Failure("TeacherId tidak valid.");
            }

            /** Langkah 3: Gunakan ProjectTo untuk langsung memproyeksikan ke DTO **/
            var courses = await _context.Courses
                .Where(c => c.Lesson.TeacherLessons.Any(tl => tl.TeacherId == teacherId)) // Memfilter course berdasarkan TeacherId
                .OrderByDescending(c => c.CreatedAt) // Urutkan course berdasarkan tanggal pembuatan secara menurun
                .ProjectTo<CourseGetDto>(_mapper.ConfigurationProvider) // Proyeksikan ke DTO menggunakan AutoMapper
                .ToListAsync(cancellationToken); // Ambil hasilnya dalam bentuk daftar

            /** Langkah 4: Periksa jika tidak ada course yang ditemukan untuk guru **/
            if (!courses.Any())
            {
                return Result<List<CourseGetDto>>.Failure("Tidak ada kursus yang ditemukan untuk Guru");
            }

            /** Langkah 5: Kembalikan hasil yang berhasil bersama dengan daftar course DTO **/
            return Result<List<CourseGetDto>>.Success(courses);
        }
    }
}