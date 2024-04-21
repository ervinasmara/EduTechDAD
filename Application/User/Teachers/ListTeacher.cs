using Application.Core;
using Application.User.DTOs;
using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.User.Teachers
{
    public class ListTeacher
    {
        public class Query : IRequest<Result<List<TeacherGetAllDto>>>
        {
            // Tidak memerlukan parameter tambahan untuk meneruskan ke query
        }

        public class Handler : IRequestHandler<Query, Result<List<TeacherGetAllDto>>>
        {
            private readonly DataContext _context;
            private readonly IMapper _mapper;

            public Handler(DataContext context, IMapper mapper)
            {
                _context = context;
                _mapper = mapper;
            }

            public async Task<Result<List<TeacherGetAllDto>>> Handle(Query request, CancellationToken cancellationToken)
            {
                // Mengambil daftar guru dari basis data, termasuk informasi mapel yang diajarkan oleh masing-masing guru.
                var teachers = await _context.Teachers
                    .Include(t => t.TeacherLessons)
                        .ThenInclude(tl => tl.Lesson) // Meng-include informasi mapel yang diajarkan oleh guru
                    .ToListAsync(cancellationToken);

                // Inisialisasi daftar DTO untuk menyimpan informasi guru.
                var teacherDtos = new List<TeacherGetAllDto>();

                // Iterasi melalui setiap guru untuk membuat DTO dan mengumpulkan informasi terkait.
                foreach (var teacher in teachers)
                {
                    var teacherDto = _mapper.Map<TeacherGetAllDto>(teacher);

                    // Ambil nama-nama mapel dari pivot TeacherLesson.
                    teacherDto.LessonNames = teacher.TeacherLessons.Select(tl => tl.Lesson.LessonName).ToList();

                    // Ambil nama-nama kelas dari pivot LessonClassRoom yang terkait dengan setiap pelajaran yang diajarkan oleh guru.
                    var classNames = teacher.TeacherLessons
                        .SelectMany(tl => tl.Lesson.LessonClassRooms.Select(lcr => lcr.ClassRoom.ClassName))
                        .Where(className => className != null) // Filter nilai null
                        .Distinct() // Hapus nilai duplikat
                        .ToList();

                    teacherDto.ClassNames = classNames;

                    teacherDtos.Add(teacherDto);
                }

                // Mengembalikan hasil yang berhasil bersama dengan daftar DTO yang berisi informasi guru.
                return Result<List<TeacherGetAllDto>>.Success(teacherDtos);
            }
        }
    }
}