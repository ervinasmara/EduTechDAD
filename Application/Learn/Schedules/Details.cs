//using Application.Core;
//using AutoMapper;
//using MediatR;
//using Microsoft.EntityFrameworkCore;
//using Persistence;

//namespace Application.Learn.Schedules
//{
//    public class Search
//    {
//        public class Query : IRequest<Result<List<ScheduleGetDto>>>
//        {
//            public int? Day { get; set; }
//            public string LessonName { get; set; }
//            public string TeacherName { get; set; }
//        }

//        public class Handler : IRequestHandler<Query, Result<List<ScheduleGetDto>>>
//        {
//            private readonly DataContext _context;
//            private readonly IMapper _mapper;

//            public Handler(DataContext context, IMapper mapper)
//            {
//                _context = context;
//                _mapper = mapper;
//            }

//            public async Task<Result<List<ScheduleGetDto>>> Handle(Query request, CancellationToken cancellationToken)
//            {
//                var query = _context.Schedules
//                    .Include(s => s.Lesson)
//                        .ThenInclude(l => l.Teacher)
//                    .Include(s => s.ClassRoom)
//                    .AsQueryable();

//                if (request.Day.HasValue)
//                {
//                    query = query.Where(s => s.Day == request.Day);
//                }

//                if (!string.IsNullOrEmpty(request.LessonName))
//                {
//                    query = query.Where(s => s.Lesson.LessonName.ToLower().Contains(request.LessonName.ToLower()));
//                }

//                if (!string.IsNullOrEmpty(request.TeacherName))
//                {
//                    query = query.Where(s => s.Lesson.Teacher.NameTeacher.ToLower().Contains(request.TeacherName.ToLower()));
//                }

//                var schedules = await query.ToListAsync(cancellationToken);
//                var scheduleDtos = _mapper.Map<List<ScheduleGetDto>>(schedules);

//                // Map ID dari Schedule ke ScheduleGetDto
//                for (int i = 0; i < schedules.Count; i++)
//                {
//                    scheduleDtos[i].Id = schedules[i].Id;

//                    // Map informasi tentang guru ke ScheduleGetDto
//                    scheduleDtos[i].TeacherSchedule = new TeacherScheduleDto
//                    {
//                        TeacherId = schedules[i].Lesson.TeacherId,
//                        NameTeacher = schedules[i].Lesson.Teacher.NameTeacher // Anggap properti nama guru ada di entitas Teacher
//                    };
//                }

//                return Result<List<ScheduleGetDto>>.Success(scheduleDtos);
//            }
//        }
//    }
//}