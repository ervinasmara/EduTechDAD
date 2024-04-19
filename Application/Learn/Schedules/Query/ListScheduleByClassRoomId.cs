using Application.Core;
using Application.Interface;
using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Learn.Schedules.Query
{
    public class ListScheduleByClassRoomId
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

            // Metode Handle() digunakan untuk menangani permintaan query dengan mencari jadwal pelajaran yang sesuai dengan ClassRoomId yang diperoleh dari token.
            public async Task<Result<List<ScheduleGetDto>>> Handle(Query request, CancellationToken cancellationToken)
            {
                // Dapatkan ClassRoomId dari token menggunakan IUserAccessor
                var classRoomIdString = _userAccessor.GetClassRoomIdFromToken();

                // Periksa jika ClassRoomId ada di token
                if (string.IsNullOrEmpty(classRoomIdString))
                {
                    return Result<List<ScheduleGetDto>>.Failure("ClassRoomId not found in token");
                }

                // Konversi ClassRoomId dari string ke Guid
                var classRoomId = Guid.Parse(classRoomIdString);

                // Dapatkan daftar jadwal pelajaran yang sesuai dengan ClassRoomId dari basis data
                var schedules = await _context.Schedules
                    .Where(s => s.ClassRoomId == classRoomId) // Memfilter jadwal berdasarkan ClassRoomId yang sudah dikonversi
                    .OrderBy(s => s.StartTime) // Mengurutkan jadwal berdasarkan waktu mulai
                    .Include(s => s.Lesson)
                        .ThenInclude(l => l.TeacherLessons)
                            .ThenInclude(tl => tl.Teacher) // Meng-include informasi guru untuk setiap pelajaran
                    .Include(s => s.ClassRoom) // Meng-include informasi ruang kelas untuk setiap jadwal
                    .ToListAsync(cancellationToken);

                // Mengonversi daftar jadwal menjadi objek ScheduleGetDto menggunakan mapper.
                var scheduleDtos = _mapper.Map<List<ScheduleGetDto>>(schedules);

                // Melakukan pemetaan ID dari jadwal ke ScheduleGetDto dan informasi guru untuk setiap jadwal.
                for (int i = 0; i < schedules.Count; i++)
                {
                    scheduleDtos[i].Id = schedules[i].Id;
                    scheduleDtos[i].LessonName = schedules[i].Lesson.LessonName;
                    scheduleDtos[i].ClassName = schedules[i].ClassRoom.ClassName;

                    // Mencari guru yang mengajar pelajaran ini di ruang kelas yang sesuai.
                    var teacherLesson = schedules[i].Lesson.TeacherLessons.FirstOrDefault(tl =>
                        tl.LessonId == schedules[i].Lesson.Id && // Memeriksa LessonId dari pengajar
                        _context.LessonClassRooms.Any(lcr => // Memeriksa keberadaan ruang kelas terkait dengan pelajaran
                            lcr.LessonId == schedules[i].Lesson.Id && // Memeriksa LessonId dalam relasi LessonClassRoom
                            lcr.ClassRoomId == classRoomId && // Menggunakan ClassRoomId yang sudah dikonversi
                            _context.TeacherClassRooms.Any(tcr => // Memeriksa keberadaan pengajar terkait dengan ruang kelas dan pelajaran
                                tcr.TeacherId == tl.TeacherId && // Memeriksa TeacherId dalam relasi TeacherClassRoom
                                tcr.ClassRoomId == lcr.ClassRoomId // Memeriksa ClassRoomId dalam relasi TeacherClassRoom
                            )
                        )
                    );
                    // Jika guru ditemukan, set nama gurunya dalam scheduleDto
                    if (teacherLesson != null)
                        scheduleDtos[i].NameTeacher = teacherLesson.Teacher.NameTeacher;
                }

                // Mengembalikan hasil yang berhasil bersama dengan daftar jadwal yang telah diproses.
                return Result<List<ScheduleGetDto>>.Success(scheduleDtos);
            }
        }
    }
}
