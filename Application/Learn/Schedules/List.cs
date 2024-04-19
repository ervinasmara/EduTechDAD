﻿using Application.Core;
using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Learn.Schedules
{
    public class List
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

            public async Task<Result<List<ScheduleGetDto>>> Handle(Query request, CancellationToken cancellationToken)
            {
                var schedules = await _context.Schedules
                    .Include(s => s.Lesson)
                        .ThenInclude(l => l.TeacherLessons)
                            .ThenInclude(tl => tl.Teacher) // Include guru dari pelajaran
                    .Include(s => s.ClassRoom)
                    .OrderBy(s => s.Day)
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
                            lcr.ClassRoomId == schedules[i].ClassRoom.Id &&
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