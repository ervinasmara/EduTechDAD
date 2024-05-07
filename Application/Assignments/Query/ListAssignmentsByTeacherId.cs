using Application.Core;
using Application.Interface;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Assignments.Query;
public class ListAssignmentsByTeacherId
{
    public class Query : IRequest<Result<List<AssignmentGetByTeacherIdDto>>>
    {
        // Tidak memerlukan parameter tambahan untuk meneruskan ke query
    }

    public class Handler : IRequestHandler<Query, Result<List<AssignmentGetByTeacherIdDto>>>
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

        public async Task<Result<List<AssignmentGetByTeacherIdDto>>> Handle(Query request, CancellationToken cancellationToken)
        {
            /** Langkah 1: Mendapatkan TeacherId dari token **/
            var teacherId = _userAccessor.GetTeacherIdFromToken();

            /** Langkah 2: Validasi TeacherId (penanganan kesalahan untuk string kosong) **/
            if (string.IsNullOrEmpty(teacherId))
            {
                return Result<List<AssignmentGetByTeacherIdDto>>.Failure("TeacherId tidak ditemukan ditoken");
            }

            try
            {
                /** Langkah 3: Membuat query untuk menemukan tugas untuk Teacher **/
                var assignments = await _context.Assignments
                    .Where(ta => ta.Course.Lesson.TeacherLessons.Any(tl => tl.TeacherId == Guid.Parse(teacherId)) && ta.Status != 0)                        // Menyaring tugas berdasarkan asosiasi Teacher
                    .OrderByDescending(a => a.CreatedAt) // Urutkan berdasarkan tanggal pembuatan (menurun)
                    .ProjectTo<AssignmentGetByTeacherIdDto>(_mapper.ConfigurationProvider) // Memetakan hasil secara efisien ke DTO menggunakan AutoMapper
                    .ToListAsync(cancellationToken);

                /** Langkah 4: Periksa hasil kosong dan kembalikan kegagalan jika tidak ada penugasan yang ditemukan **/
                if (assignments == null || !assignments.Any())
                {
                    return Result<List<AssignmentGetByTeacherIdDto>>.Failure("Tidak ada tugas yang ditemukan untuk TeacherId yang diberikan");
                }

                /** Langkah 5: Kembalikan hasil yang berhasil dengan daftar Assignments **/
                return Result<List<AssignmentGetByTeacherIdDto>>.Success(assignments);
            }
            catch (Exception ex)
            {
                /** 6. Menangkap pengecualian potensial selama pengambilan data dan kegagalan pengembalian dengan pesan kesalahan **/
                return Result<List<AssignmentGetByTeacherIdDto>>.Failure($"Gagal mengambil tugas: {ex.Message}");
            }
        }
    }
}