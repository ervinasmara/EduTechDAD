using Application.Core;
using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Learn.Schedules.Query
{
    public class ListSchedule
    {
        public class Query : IRequest<Result<List<ScheduleGetDto>>>
        {
            // Tidak diperlukan parameter tambahan
        }

        public class Handler : IRequestHandler<Query, Result<List<ScheduleGetDto>>>
        {
            private readonly DataContext _context;
            private readonly IMapper _mapper;

            public Handler(DataContext context, IMapper mapper)
            {
                _context = context;
                _mapper = mapper;
            }

            // Metode Handle() digunakan untuk menangani permintaan query dengan mencari semua jadwal pelajaran.
            public async Task<Result<List<ScheduleGetDto>>> Handle(Query request, CancellationToken cancellationToken)
            {
                // Mengambil semua jadwal pelajaran dari basis data, termasuk informasi tentang pelajaran, guru, dan ruang kelas.
                var schedules = await _context.Schedules
                    .Include(s => s.Lesson) // Meng-include informasi pelajaran dalam jadwal
                        .ThenInclude(l => l.TeacherLessons) // Meng-include daftar pengajar untuk setiap pelajaran
                            .ThenInclude(tl => tl.Teacher) // Meng-include informasi guru untuk setiap pengajar
                    .Include(s => s.ClassRoom) // Meng-include informasi ruang kelas
                    .OrderBy(s => s.Day) // Mengurutkan jadwal menurut hari
                    .ToListAsync(cancellationToken);

                // Mengonversi daftar jadwal menjadi objek ScheduleGetDto menggunakan mapper.
                var scheduleDtos = _mapper.Map<List<ScheduleGetDto>>(schedules);

                // Melakukan pengulangan melalui setiap jadwal untuk melakukan pemetaan ID jadwal, nama pelajaran, dan nama ruang kelas dalam objek ScheduleGetDto.
                for (int i = 0; i < schedules.Count; i++)
                {
                    scheduleDtos[i].Id = schedules[i].Id;
                    scheduleDtos[i].LessonName = schedules[i].Lesson.LessonName;
                    scheduleDtos[i].ClassName = schedules[i].ClassRoom.ClassName;

                    // Mencari guru yang mengajar pelajaran ini di ruang kelas tertentu.
                    var teacherLesson = schedules[i].Lesson.TeacherLessons.FirstOrDefault(tl =>
                        tl.LessonId == schedules[i].Lesson.Id && // Memastikan ID pelajaran sesuai
                        _context.LessonClassRooms.Any(lcr =>
                            lcr.LessonId == schedules[i].Lesson.Id && // Memastikan ID pelajaran sesuai
                            lcr.ClassRoomId == schedules[i].ClassRoom.Id && // Memastikan ID ruang kelas sesuai
                            _context.TeacherClassRooms.Any(tcr =>
                                tcr.TeacherId == tl.TeacherId && // Memastikan ID guru sesuai
                                tcr.ClassRoomId == lcr.ClassRoomId // Memastikan ID ruang kelas sesuai
                            )
                        )
                    );
                    // Jika guru ditemukan, nama guru ditetapkan dalam ScheduleGetDto.
                    if (teacherLesson != null)
                        scheduleDtos[i].NameTeacher = teacherLesson.Teacher.NameTeacher;
                }

                // Mengembalikan hasil yang berhasil bersama dengan daftar jadwal yang telah diproses.
                return Result<List<ScheduleGetDto>>.Success(scheduleDtos);
            }
        }
    }
}