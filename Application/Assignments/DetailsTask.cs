using Application.Core;
using Application.Interface;
using Application.Learn.GetFileName;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Assignments
{
    public class DetailsTask
    {
        public class Query : IRequest<Result<AssignmentGetDto>>
        {
            public Guid AssignmentId { get; set; }
        }

        public class Handler : IRequestHandler<Query, Result<AssignmentGetDto>>
        {
            private readonly DataContext _context;
            private readonly IUserAccessor _userAccessor;

            public Handler(DataContext context, IUserAccessor userAccessor)
            {
                _context = context;
                _userAccessor = userAccessor;
            }

            public async Task<Result<AssignmentGetDto>> Handle(Query request, CancellationToken cancellationToken)
            {
                try
                {
                    var studentId = _userAccessor.GetStudentIdFromToken(); // Dapatkan StudentId dari token

                    // Dapatkan assignment berdasarkan AssignmentId
                    var assignment = await _context.Assignments
                        .Include(a => a.Course)
                        .ThenInclude(c => c.Lesson) // Termasuk informasi Lesson dalam query
                        .FirstOrDefaultAsync(a => a.Id == request.AssignmentId, cancellationToken);

                    // Periksa apakah assignment ditemukan
                    if (assignment == null)
                    {
                        return Result<AssignmentGetDto>.Failure("Assignment not found.");
                    }

                    // Dapatkan nama pelajaran dari tugas yang terkait
                    var lessonName = assignment.Course?.Lesson?.LessonName;

                    // Dapatkan nama-nama kelas yang terkait dengan assignment
                    var classNames = await GetClassNamesForAssignment(request.AssignmentId, cancellationToken);

                    // Cek status AssignmentSubmission berdasarkan StudentId
                    var submission = await _context.AssignmentSubmissions
                        .FirstOrDefaultAsync(s => s.AssignmentId == request.AssignmentId && s.StudentId == Guid.Parse(studentId), cancellationToken);

                    var today = DateOnly.FromDateTime(DateTime.Now); // Dapatkan tanggal hari ini tanpa informasi waktu
                    var deadline = assignment.AssignmentDeadline; // Deadline dari tugas

                    var status = submission == null
                        ? today > deadline ? "Kamu Terlambat" : "Belum Dikerjakan"
                        : submission.Grade.HasValue ? "Sudah Dinilai" : "Sudah Dikerjakan";

                    // Buat DTO AssignmentGetDto
                    var assignmentDto = new AssignmentGetDto
                    {
                        Id = assignment.Id,
                        AssignmentName = assignment.AssignmentName,
                        AssignmentDate = assignment.AssignmentDate,
                        AssignmentDeadline = assignment.AssignmentDeadline,
                        AssignmentDescription = assignment.AssignmentDescription,
                        AssignmentLink = assignment.AssignmentLink,
                        LessonName = lessonName,
                        AssignmentFileData = assignment.FileData,
                        ClassNames = classNames,
                        AssignmentStatus = status
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

                    return Result<AssignmentGetDto>.Success(assignmentDto);
                }
                catch (Exception ex)
                {
                    // Tangani pengecualian dan kembalikan pesan kesalahan yang sesuai
                    return Result<AssignmentGetDto>.Failure($"Failed to retrieve assignment: {ex.Message}");
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