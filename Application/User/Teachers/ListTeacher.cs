using Application.Core;
using Application.Learn.Lessons;
using Application.User.DTOs;
using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.User.Teacher
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
                var teachers = await _context.Teachers
                    .Include(t => t.TeacherLessons)
                        .ThenInclude(tl => tl.Lesson) // Include mapel yang diajar oleh guru
                    .ToListAsync(cancellationToken);

                var teacherDtos = new List<TeacherGetAllDto>();

                foreach (var teacher in teachers)
                {
                    var teacherDto = _mapper.Map<TeacherGetAllDto>(teacher);

                    // Ambil LessonNames dari pivot TeacherLesson
                    teacherDto.LessonNames = teacher.TeacherLessons.Select(tl => tl.Lesson.LessonName).ToList();

                    // Ambil ClassNames dari pivot LessonClassRoom yang terkait dengan setiap Lesson
                    var classNames = teacher.TeacherLessons
                        .SelectMany(tl => tl.Lesson.LessonClassRooms.Select(lcr => lcr.ClassRoom.ClassName))
                        .Where(className => className != null) // Filter null values
                        .Distinct() // Hapus nilai duplikat
                        .ToList();

                    teacherDto.ClassNames = classNames;

                    teacherDtos.Add(teacherDto);
                }

                return Result<List<TeacherGetAllDto>>.Success(teacherDtos);
            }
        }
    }
}