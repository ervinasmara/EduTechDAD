using Application.Core;
using Application.Interface;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Learn.Lessons.Query;
public class LessonByTeacherId
{
    public class Query : IRequest<Result<List<LessonGetByTeacherIdOrClassRoomIdDto>>>
    {
        // Tidak memerlukan parameter tambahan karena TeacherId diambil dari token
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
            /** Langkah 1: Dapatkan TeacherId dari token siswa **/
            var teacherIdString = _userAccessor.GetTeacherIdFromToken();

            /** Langkah 2: Cek apakah TeacherId ada di dalam token **/
            if (string.IsNullOrEmpty(teacherIdString))
            {
                return Result<List<LessonGetByTeacherIdOrClassRoomIdDto>>.Failure("TeacherId tidak ditemukan ditoken");
            }

            /** Langkah 3: Validasi format TeacherId **/
            if (!Guid.TryParse(teacherIdString, out var teacherId))
            {
                return Result<List<LessonGetByTeacherIdOrClassRoomIdDto>>.Failure("TeacherId tidak valid.");
            }

            /** Langkah 4: Dapatkan daftar Lesson berdasarkan TeacherId **/
            var lessons = await _context.Lessons
                .Where(l => l.TeacherLessons.Any(tl => tl.TeacherId == teacherId) && l.Status != 0)
                .ProjectTo<LessonGetByTeacherIdOrClassRoomIdDto>(_mapper.ConfigurationProvider)
                .ToListAsync(cancellationToken);

            /** Langkah 5: Cek apakah Lesson ada dari TeacherId yang ada **/
            if (!lessons.Any())
            {
                return Result<List<LessonGetByTeacherIdOrClassRoomIdDto>>.Failure("Tidak ada pelajaran yang ditemukan untuk TeacherId yang diberikan");
            }

            return Result<List<LessonGetByTeacherIdOrClassRoomIdDto>>.Success(lessons);
        }
    }
}