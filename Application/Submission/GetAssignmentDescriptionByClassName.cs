using Application.Core;
using Application.Interface;
using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Submission
{
    public class GetAssignmentDescriptionByClassName
    {
        public class Query : IRequest<Result<SubmissionGetAssignmentNameByClassNameDto>>
        {
            public string ClassName { get; set; }
        }

        public class Handler : IRequestHandler<Query, Result<SubmissionGetAssignmentNameByClassNameDto>>
        {
            private readonly DataContext _context;
            private readonly IUserAccessor _userAccessor;
            private readonly IMapper _mapper;

            public Handler(DataContext context, IUserAccessor userAccessor, IMapper mapper)
            {
                _context = context;
                _userAccessor = userAccessor;
                _mapper = mapper;
            }

            public async Task<Result<SubmissionGetAssignmentNameByClassNameDto>> Handle(Query request, CancellationToken cancellationToken)
            {
                try
                {
                    // Retrieve TeacherId from token
                    var teacherId = Guid.Parse(_userAccessor.GetTeacherIdFromToken()); // Assuming a method to retrieve current user's id from token

                    // Find all TeacherClassRooms for the teacher
                    var teacherClassRooms = await _context.TeacherClassRooms
                        .Where(tc => tc.TeacherId == teacherId)
                        .Select(tc => tc.ClassRoomId)
                        .ToListAsync();

                    // Find AssignmentClassRoom by ClassRoomId and ClassName
                    var assignmentClassRoom = await _context.AssignmentClassRooms
                        .Include(acr => acr.Assignment)
                            .ThenInclude(a => a.Course)
                                .ThenInclude(c => c.Lesson)
                        .FirstOrDefaultAsync(acr => teacherClassRooms.Contains(acr.ClassRoomId) && acr.ClassRoom.ClassName == request.ClassName);

                    if (assignmentClassRoom == null)
                        return Result<SubmissionGetAssignmentNameByClassNameDto>.Failure($"No assignments found for class {request.ClassName}");

                    // Map AssignmentClassRoom to DTO
                    var assignmentDto = _mapper.Map<SubmissionGetAssignmentNameByClassNameDto>(assignmentClassRoom.Assignment);
                    assignmentDto.ClassName = request.ClassName; // Set ClassName from query parameter

                    // Combine LessonName, CourseName, and AssignmentName
                    assignmentDto.AssignmentNameLessonCourse = $"{assignmentClassRoom.Assignment.Course.Lesson.LessonName} - {assignmentClassRoom.Assignment.Course.CourseName} - {assignmentClassRoom.Assignment.AssignmentName}";

                    return Result<SubmissionGetAssignmentNameByClassNameDto>.Success(assignmentDto);
                }
                catch (Exception ex)
                {
                    // Handle exception
                    return Result<SubmissionGetAssignmentNameByClassNameDto>.Failure($"Failed to get assignment description: {ex.Message}");
                }
            }
        }
    }
}
