using Application.Core;
using Application.Interface;
using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Learn.Lessons
{
    public class LessonByClassRoomId
    {
        public class Query : IRequest<Result<List<LessonDto>>>
        {
            // Tidak diperlukan parameter tambahan karena ClassroomId diambil dari token
        }

        public class Handler : IRequestHandler<Query, Result<List<LessonDto>>>
        {
            private readonly DataContext _context;
            private readonly IUserAccessor _userAccessor;

            public Handler(DataContext context, IMapper mapper, IUserAccessor userAccessor)
            {
                _context = context;
                _userAccessor = userAccessor;
            }

            public async Task<Result<List<LessonDto>>> Handle(Query request, CancellationToken cancellationToken)
            {
                // Dapatkan ClassroomId dari token
                var classRoomIdString = _userAccessor.GetClassRoomIdFromToken();

                // Periksa jika ClassroomId ada di token
                if (string.IsNullOrEmpty(classRoomIdString))
                {
                    return Result<List<LessonDto>>.Failure("ClassRoomId not found in token");
                }

                if (!Guid.TryParse(classRoomIdString, out var classRoomId))
                {
                    return Result<List<LessonDto>>.Failure("ClassRoomId not valid.");
                }

                // Dapatkan daftar LessonIds berdasarkan ClassroomId
                var lessonIds = _context.LessonClassRooms
                    .Where(lcr => lcr.ClassRoomId == classRoomId)
                    .Select(lcr => lcr.LessonId)
                    .ToList(); // Menggunakan ToList() untuk mengubah IQueryable<Guid> menjadi List<Guid>

                // Dapatkan detail pelajaran berdasarkan LessonIds
                var lessons = await _context.Lessons
                    .Where(l => lessonIds.Contains(l.Id))
                    .Select(l => new LessonDto
                    {
                        LessonName = l.LessonName,
                        UniqueNumberOfLesson = l.UniqueNumberOfLesson
                    })
                    .ToListAsync(cancellationToken);

                return Result<List<LessonDto>>.Success(lessons);
            }
        }
    }
}