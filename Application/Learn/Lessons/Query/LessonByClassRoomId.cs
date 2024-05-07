using Application.Core;
using Application.Interface;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Learn.Lessons.Query;
public class LessonByClassRoomId
{
    public class Query : IRequest<Result<List<LessonGetByTeacherIdOrClassRoomIdDto>>>
    {
        // Tidak diperlukan parameter tambahan karena ClassroomId diambil dari token
    }

    public class Handler : IRequestHandler<Query, Result<List<LessonGetByTeacherIdOrClassRoomIdDto>>>
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

        public async Task<Result<List<LessonGetByTeacherIdOrClassRoomIdDto>>> Handle(Query request, CancellationToken cancellationToken)
        {
            /** Langkah 1: Dapatkan ClassRoomId dari token siswa **/
            var classRoomIdString = _userAccessor.GetClassRoomIdFromToken();

            /** Langkah 2: Cek apakah ClassRoomId ada di dalam token **/
            if (string.IsNullOrEmpty(classRoomIdString))
            {
                return Result<List<LessonGetByTeacherIdOrClassRoomIdDto>>.Failure("ClassRoomId tidak ditemukan ditoken");
            }

            /** Langkah 3: Validasi format ClassRoomId **/
            if (!Guid.TryParse(classRoomIdString, out var classRoomId))
            {
                return Result<List<LessonGetByTeacherIdOrClassRoomIdDto>>.Failure("ClassRoomId tidak valid.");
            }

            /** Langkah 4: Dapatkan daftar Lesson berdasarkan ClassRoomId **/
            var lessons = await _context.Lessons
                .Where(l => l.ClassRoomId == classRoomId && l.Status != 0) // Filter lessons by ClassRoomId
                .ProjectTo<LessonGetByTeacherIdOrClassRoomIdDto>(_mapper.ConfigurationProvider)
                .ToListAsync(cancellationToken);

            /** Langkah 5: Cek apakah Lesson ada dari ClassRoomId yang ada **/
            if (!lessons.Any())
            {
                return Result<List<LessonGetByTeacherIdOrClassRoomIdDto>>.Failure("Tidak ada pelajaran yang ditemukan untuk ClassRoomId yang diberikan");
            }

            /** Langkah 6: Kembalikan hasil yang berhasil dengan daftar LessonGetByTeacherIdOrClassRoomIdDto **/
            return Result<List<LessonGetByTeacherIdOrClassRoomIdDto>>.Success(lessons);
        }
    }
}