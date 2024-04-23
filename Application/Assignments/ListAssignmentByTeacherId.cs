using Application.Core;
using Application.Interface;
using Application.Learn.GetFileName;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Assignments
{
    public class ListAssignmentByTeacherId
    {
        public class Query : IRequest<Result<List<AssignmentGetByTeacherIdDto>>>
        {
            // Tidak memerlukan parameter tambahan untuk meneruskan ke query
        }

        public class Handler : IRequestHandler<Query, Result<List<AssignmentGetByTeacherIdDto>>>
        {
            private readonly DataContext _context;
            private readonly IUserAccessor _userAccessor;

            public Handler(DataContext context, IUserAccessor userAccessor)
            {
                _context = context;
                _userAccessor = userAccessor;
            }

            public async Task<Result<List<AssignmentGetByTeacherIdDto>>> Handle(Query request, CancellationToken cancellationToken)
            {
                var teacherId = _userAccessor.GetTeacherIdFromToken();

                // Periksa apakah teacherId valid
                if (string.IsNullOrEmpty(teacherId))
                {
                    return Result<List<AssignmentGetByTeacherIdDto>>.Failure("TeacherId not found in token.");
                }

                try
                {
                    // Dapatkan semua tugas yang terkait dengan guru
                    var assignments = await _context.TeacherAssignments
                        .Include(ta => ta.Assignment)
                        .ThenInclude(a => a.Course)
                        .ThenInclude(c => c.Lesson)
                        .Where(ta => ta.TeacherId == Guid.Parse(teacherId))
                        .Select(ta => ta.Assignment)
                        .OrderByDescending(a => a.CreatedAt)
                        .ToListAsync(cancellationToken);

                    var assignmentDtos = new List<AssignmentGetByTeacherIdDto>();

                    foreach (var assignment in assignments)
                    {
                        var lessonName = assignment.Course?.Lesson?.LessonName;
                        var classNames = await GetClassNamesForAssignment(assignment.Id, cancellationToken);

                        var assignmentDto = new AssignmentGetByTeacherIdDto
                        {
                            Id = assignment.Id,
                            AssignmentName = assignment.AssignmentName,
                            AssignmentDate = assignment.AssignmentDate,
                            AssignmentDeadline = assignment.AssignmentDeadline,
                            AssignmentDescription = assignment.AssignmentDescription,
                            AssignmentLink = assignment.AssignmentLink,
                            LessonName = lessonName,
                            AssignmentFileData = assignment.FileData,
                            ClassNames = classNames
                        };

                        if (!string.IsNullOrEmpty(assignment.AssignmentName) && assignment.FileData != null)
                        {
                            assignmentDto.AssignmentFileName = $"{assignment.AssignmentName}.{GetFileExtension.FileExtensionHelper(assignment.FileData)}";
                        }
                        else
                        {
                            assignmentDto.AssignmentFileName = "UnknownFileName";
                        }

                        assignmentDtos.Add(assignmentDto);
                    }

                    return Result<List<AssignmentGetByTeacherIdDto>>.Success(assignmentDtos);
                }
                catch (Exception ex)
                {
                    return Result<List<AssignmentGetByTeacherIdDto>>.Failure($"Failed to retrieve assignments: {ex.Message}");
                }
            }

            private async Task<ICollection<string>> GetClassNamesForAssignment(Guid assignmentId, CancellationToken cancellationToken)
            {
                var classNames = await _context.AssignmentClassRooms
                    .Where(acr => acr.AssignmentId == assignmentId)
                    .Select(acr => acr.ClassRoom.ClassName)
                    .ToListAsync(cancellationToken);

                return classNames;
            }
        }
    }
}
