using Application.Core;
using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Learn.Schedules.Query
{
    public class ListScheduleById
    {
        public class Query : IRequest<Result<ScheduleGetDto>>
        {
            public Guid ScheduleId { get; set; }
        }

        public class Handler : IRequestHandler<Query, Result<ScheduleGetDto>>
        {
            private readonly DataContext _context;
            private readonly IMapper _mapper;

            public Handler(DataContext context, IMapper mapper)
            {
                _context = context;
                _mapper = mapper;
            }

            // Metode Handle() digunakan untuk menangani permintaan query dengan mencari jadwal pelajaran tertentu.
            public async Task<Result<ScheduleGetDto>> Handle(Query request, CancellationToken cancellationToken)
            {
                // Mengambil jadwal pelajaran dari basis data berdasarkan ID jadwal yang diminta.
                var schedule = await _context.Schedules
                    .Include(s => s.Lesson) // Meng-include informasi pelajaran dalam jadwal
                        .ThenInclude(l => l.TeacherLessons) // Meng-include daftar pengajar untuk setiap pelajaran
                            .ThenInclude(tl => tl.Teacher) // Meng-include informasi guru untuk setiap pengajar
                    .Include(s => s.ClassRoom) // Meng-include informasi ruang kelas
                    .FirstOrDefaultAsync(s => s.Id == request.ScheduleId, cancellationToken);

                // Jika jadwal tidak ditemukan, kembalikan hasil kegagalan dengan pesan yang sesuai.
                if (schedule == null)
                {
                    return Result<ScheduleGetDto>.Failure("Lesson Not Found.");
                }

                // Mengonversi objek jadwal menjadi objek ScheduleGetDto menggunakan mapper.
                var scheduleDto = _mapper.Map<ScheduleGetDto>(schedule);

                // Set nilai ClassName dan LessonName dari scheduleDto, menggunakan null-conditional operator untuk mengatasi kemungkinan ClassRoom atau Lesson null.
                scheduleDto.ClassName = schedule.ClassRoom?.ClassName;
                scheduleDto.LessonName = schedule.Lesson?.LessonName;

                // Mencari guru yang mengajar pelajaran ini di ruang kelas tertentu.
                var teacherLesson = schedule.Lesson?.TeacherLessons.FirstOrDefault(tl => // Mencari pengajar pertama dari pelajaran yang terkait dengan jadwal (gunakan null-conditional operator untuk menghindari NullReferenceException jika Lesson null)
                    tl.LessonId == schedule.Lesson.Id && // Memeriksa apakah ID pelajaran pengajar sesuai dengan ID pelajaran jadwal yang terkait
                    _context.LessonClassRooms.Any(lcr => // Memeriksa apakah ada setidaknya satu ruang kelas terkait dengan pelajaran
                        lcr.LessonId == schedule.Lesson.Id && // Memeriksa apakah ID pelajaran dalam relasi LessonClassRoom sesuai dengan ID pelajaran jadwal yang terkait
                        lcr.ClassRoomId == schedule.ClassRoom.Id && // Memeriksa apakah ID ruang kelas dalam relasi LessonClassRoom sesuai dengan ID ruang kelas jadwal yang terkait
                        _context.TeacherClassRooms.Any(tcr => // Memeriksa apakah ada pengajar terkait dengan ruang kelas dan pelajaran yang sama
                            tcr.TeacherId == tl.TeacherId && // Memeriksa apakah ID guru dalam relasi TeacherClassRoom sesuai dengan ID guru dalam relasi TeacherLesson
                            tcr.ClassRoomId == lcr.ClassRoomId // Memeriksa apakah ID ruang kelas dalam relasi TeacherClassRoom sesuai dengan ID ruang kelas dalam relasi LessonClassRoom
                        )
                    )
                );

                // Jika guru ditemukan, nama gurunya ditetapkan dalam scheduleDto.
                if (teacherLesson != null)
                    scheduleDto.NameTeacher = teacherLesson.Teacher.NameTeacher;

                // Mengembalikan hasil yang berhasil bersama dengan jadwal pelajaran yang telah diproses.
                return Result<ScheduleGetDto>.Success(scheduleDto);
            }
        }
    }
}