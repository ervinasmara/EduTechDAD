using Application.Core;
using Application.Interface;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.ClassRooms.Query
{
    public class ClassRoomTeacher
    {
        public class Query : IRequest<Result<ClassRoomTeacherDto>>
        {
            // Tidak memerlukan properti karena hanya mengambil informasi dari token
        }

        public class Handler : IRequestHandler<Query, Result<ClassRoomTeacherDto>>
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

            public async Task<Result<ClassRoomTeacherDto>> Handle(Query request, CancellationToken cancellationToken)
            {
                var teacherId = _userAccessor.GetTeacherIdFromToken();

                if (teacherId == null)
                {
                    return Result<ClassRoomTeacherDto>.Failure("Teacher ID not found in token");
                }

                var teacherLessons = await _context.TeacherLessons
                    .Where(tl => tl.TeacherId == Guid.Parse(teacherId))
                    .Select(tl => tl.LessonId)
                    .ToListAsync();

                var classRooms = await _context.ClassRooms
                    .Where(classRoom => classRoom.Lessons.Any(lesson => teacherLessons.Contains(lesson.Id)))
                    .ProjectTo<ClassRoomDto>(_mapper.ConfigurationProvider) // Gunakan ProjectTo untuk memetakan ke DTO
                    .ToListAsync();

                var classRoomTeacherDto = new ClassRoomTeacherDto
                {
                    TeacherId = teacherId,
                    ClassRooms = classRooms
                };

                return Result<ClassRoomTeacherDto>.Success(classRoomTeacherDto);
            }
        }
    }
}