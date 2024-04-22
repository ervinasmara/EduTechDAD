using Application.Core;
using Application.Learn.GetFileName;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Assignments
{
    public class DetailsTask
    {
        public class Query : IRequest<Result<AssignmentGetByTeacherIdDto>>
        {
            public Guid AssignmentId { get; set; }
        }

        public class Handler : IRequestHandler<Query, Result<AssignmentGetByTeacherIdDto>>
        {
            private readonly DataContext _context;

            public Handler(DataContext context)
            {
                _context = context;
            }

            public async Task<Result<AssignmentGetByTeacherIdDto>> Handle(Query request, CancellationToken cancellationToken)
            {
                try
                {
                    // Dapatkan assignment berdasarkan AssignmentId
                    var assignment = await _context.Assignments
                        .Include(a => a.Course)
                        .ThenInclude(c => c.Lesson) // Termasuk informasi Lesson dalam query
                        .FirstOrDefaultAsync(a => a.Id == request.AssignmentId, cancellationToken);

                    // Periksa apakah assignment ditemukan
                    if (assignment == null)
                    {
                        return Result<AssignmentGetByTeacherIdDto>.Failure("Assignment not found.");
                    }

                    // Dapatkan nama pelajaran dari tugas yang terkait
                    var lessonName = assignment.Course?.Lesson?.LessonName;

                    // Dapatkan nama-nama kelas yang terkait dengan assignment
                    var classNames = await GetClassNamesForAssignment(request.AssignmentId, cancellationToken);

                    // Buat DTO AssignmentGetByTeacherIdDto
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

                    // Set AssignmentFileName berdasarkan AssignmentName dan AssignmentFileData extension
                    if (!string.IsNullOrEmpty(assignment.AssignmentName) && assignment.FileData != null)
                    {
                        assignmentDto.AssignmentFileName = $"{assignment.AssignmentName}.{GetFileExtension.FileExtensionHelper(assignment.FileData)}";
                    }
                    else
                    {
                        // Handle nilai null dengan benar
                        assignmentDto.AssignmentFileName = "UnknownFileName";
                    }

                    return Result<AssignmentGetByTeacherIdDto>.Success(assignmentDto);
                }
                catch (Exception ex)
                {
                    // Tangani pengecualian dan kembalikan pesan kesalahan yang sesuai
                    return Result<AssignmentGetByTeacherIdDto>.Failure($"Failed to retrieve assignment: {ex.Message}");
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