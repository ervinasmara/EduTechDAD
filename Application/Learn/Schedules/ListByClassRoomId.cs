using Application.Core;
using Application.Interface;
using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Learn.Schedules
{
    public class ListByClassRoomId
    {
        public class Query : IRequest<Result<List<ScheduleGetDto>>>
        {
            public string ClassRoomId { get; set; }
        }

        public class Handler : IRequestHandler<Query, Result<List<ScheduleGetDto>>>
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

            public async Task<Result<List<ScheduleGetDto>>> Handle(Query request, CancellationToken cancellationToken)
            {
                // Dapatkan ClassRoomId dari token menggunakan IUserAccessor
                var classRoomIdString = _userAccessor.GetClassRoomIdFromToken();

                // Periksa jika ClassRoomId ada di token
                if (string.IsNullOrEmpty(classRoomIdString))
                {
                    return Result<List<ScheduleGetDto>>.Failure("ClassRoomId tidak ditemukan dalam token.");
                }

                // Konversi ClassRoomId dari string ke Guid
                var classRoomId = Guid.Parse(classRoomIdString);

                // Dapatkan daftar jadwal pelajaran yang sesuai dengan ClassRoomId
                var schedules = await _context.Schedules
                    .Where(s => s.ClassRoomId == classRoomId) // Gunakan ClassRoomId yang sudah dikonversi
                    .OrderBy(s => s.StartTime)
                    .Include(s => s.Lesson)
                        .ThenInclude(l => l.TeacherLessons)
                            .ThenInclude(tl => tl.Teacher) // Include guru dari pelajaran
                    .Include(s => s.ClassRoom)
                    .ToListAsync(cancellationToken);

                var scheduleDtos = _mapper.Map<List<ScheduleGetDto>>(schedules);

                // Map ID dari Schedule ke ScheduleGetDto dan informasi guru
                for (int i = 0; i < schedules.Count; i++)
                {
                    scheduleDtos[i].Id = schedules[i].Id;
                    scheduleDtos[i].LessonName = schedules[i].Lesson.LessonName;
                    scheduleDtos[i].ClassName = schedules[i].ClassRoom.ClassName;

                    // Cari guru yang mengajar mapel ini dalam kelas yang bersangkutan
                    var teacherLesson = schedules[i].Lesson.TeacherLessons.FirstOrDefault(tl =>
                        tl.LessonId == schedules[i].Lesson.Id &&
                        _context.LessonClassRooms.Any(lcr =>
                            lcr.LessonId == schedules[i].Lesson.Id &&
                            lcr.ClassRoomId == classRoomId && // Gunakan ClassRoomId yang sudah dikonversi
                            _context.TeacherClassRooms.Any(tcr =>
                                tcr.TeacherId == tl.TeacherId &&
                                tcr.ClassRoomId == lcr.ClassRoomId
                            )
                        )
                    );
                    // Jika guru ditemukan, set nama guru dalam scheduleDto
                    if (teacherLesson != null)
                        scheduleDtos[i].NameTeacher = teacherLesson.Teacher.NameTeacher;
                }

                return Result<List<ScheduleGetDto>>.Success(scheduleDtos);
            }
        }

    }
}
