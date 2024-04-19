using Application.Core;
using Application.User.DTOs;
using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Learn.Lessons
{
    public class List
    {
        public class Query : IRequest<Result<List<LessonGetAllDto>>>
        {
            // Tidak memerlukan parameter tambahan
        }

        public class Handler : IRequestHandler<Query, Result<List<LessonGetAllDto>>>
        {
            private readonly DataContext _context;

            public Handler(DataContext context, IMapper mapper)
            {
                _context = context;
            }

            // Metode Handle() digunakan untuk menangani permintaan query dengan mencari semua pelajaran beserta nama-nama guru yang mengajarinya.
            public async Task<Result<List<LessonGetAllDto>>> Handle(Query request, CancellationToken cancellationToken)
            {
                // Mengambil semua pelajaran dari basis data dan mengonversinya menjadi objek LessonGetAllDto yang sesuai.
                var lessonsWithTeacherNames = await _context.Lessons
                    .Select(lesson => new LessonGetAllDto
                    {
                        // Menyalin properti pelajaran ke objek DTO baru.
                        Id = lesson.Id,
                        LessonName = lesson.LessonName,
                        UniqueNumberOfLesson = lesson.UniqueNumberOfLesson,
                        // Mengambil nama-nama kelas di mana pelajaran diajarkan.
                        ClassNames = _context.LessonClassRooms
                            .Where(lcr => lcr.LessonId == lesson.Id)
                            .Select(lcr => lcr.ClassRoom.ClassName)
                            .ToList(),
                        // Mengambil informasi tentang guru yang mengajar pelajaran ini.
                        TeacherLesson = _context.TeacherLessons
                            .Where(tl => tl.LessonId == lesson.Id)
                            .Select(tl => new TeacherLessonGetAllDto
                            {
                                // Mengambil nama guru yang mengajar.
                                NameTeacher = tl.Teacher.NameTeacher,
                                // Mengambil nama-nama kelas yang diajarkan oleh guru tersebut.
                                ClassNames = _context.TeacherClassRooms
                                    .Where(tc => tc.TeacherId == tl.TeacherId)
                                    .Select(tc => tc.ClassRoom.ClassName)
                                    .ToList()
                            })
                            .ToList()
                    })
                    .ToListAsync(cancellationToken);

                // Mengembalikan hasil yang berhasil beserta daftar pelajaran dengan nama-nama guru yang mengajarinya.
                return Result<List<LessonGetAllDto>>.Success(lessonsWithTeacherNames);
            }
        }
    }
}