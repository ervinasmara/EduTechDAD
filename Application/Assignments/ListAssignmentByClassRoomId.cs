using Application.Core;
using Application.Interface;
using Application.Learn.GetFileName;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Assignments
{
    public class ListAssignmentByClassRoomId
    {
        public class Query : IRequest<Result<List<AssignmentGetByClassRoomIdDto>>>
        {
            // Tidak memerlukan parameter tambahan untuk meneruskan ke query
        }

        public class Handler : IRequestHandler<Query, Result<List<AssignmentGetByClassRoomIdDto>>>
        {
            private readonly DataContext _context;
            private readonly IUserAccessor _userAccessor;

            public Handler(DataContext context, IUserAccessor userAccessor)
            {
                _context = context;
                _userAccessor = userAccessor;
            }

            public async Task<Result<List<AssignmentGetByClassRoomIdDto>>> Handle(Query request, CancellationToken cancellationToken)
            {
                var classRoomId = _userAccessor.GetClassRoomIdFromToken();

                // Periksa apakah classRoomId valid
                if (string.IsNullOrEmpty(classRoomId))
                {
                    return Result<List<AssignmentGetByClassRoomIdDto>>.Failure("ClassRoomId not found in token.");
                }

                var studentId = _userAccessor.GetStudentIdFromToken(); // Dapatkan StudentId dari token

                // Periksa apakah studentId valid
                if (string.IsNullOrEmpty(studentId))
                {
                    return Result<List<AssignmentGetByClassRoomIdDto>>.Failure("StudentId not found in token.");
                }

                try
                {
                    // Dapatkan semua AssignmentId yang terkait dengan ClassRoomId
                    var assignmentIds = await _context.AssignmentClassRooms
                        .Where(acr => acr.ClassRoomId == Guid.Parse(classRoomId))
                        .Select(acr => acr.AssignmentId)
                        .ToListAsync(cancellationToken);

                    // Dapatkan semua tugas yang terkait dengan AssignmentId yang ditemukan
                    var assignments = await _context.Assignments
                        .Include(a => a.Course)
                        .ThenInclude(c => c.Lesson) // Termasuk informasi Lesson dalam query
                        .Where(a => assignmentIds.Contains(a.Id))
                        .OrderByDescending(a => a.CreatedAt)
                        .ToListAsync(cancellationToken);

                    var assignmentDtos = new List<AssignmentGetByClassRoomIdDto>();

                    foreach (var assignment in assignments)
                    {
                        // Dapatkan nama pelajaran dari tugas yang terkait
                        var lessonName = assignment.Course.Lesson.LessonName;

                        // Buat DTO AssignmentGetByClassRoomIdDto
                        var assignmentDto = new AssignmentGetByClassRoomIdDto
                        {
                            Id = assignment.Id,
                            AssignmentName = assignment.AssignmentName,
                            AssignmentDate = assignment.AssignmentDate,
                            AssignmentDeadline = assignment.AssignmentDeadline,
                            AssignmentDescription = assignment.AssignmentDescription,
                            AssignmentLink = assignment.AssignmentLink,
                            LessonName = lessonName,
                            AssignmentFileData = assignment.FileData
                        };


                        // Set AssignmentFileName berdasarkan AssignmentName dan AssignmentFileData extension
                        if (!string.IsNullOrEmpty(assignment.AssignmentName) && assignment.FileData != null)
                        {
                            assignmentDto.AssignmentFileName = $"{assignment.AssignmentName}.{GetFileExtension.FileExtensionHelper(assignment.FileData)}";
                        }
                        else
                        {
                            // Handle nilai null dengan benar
                            assignmentDto.AssignmentFileName = "No File";
                        }

                        var submission = await _context.AssignmentSubmissions
                            .FirstOrDefaultAsync(s => s.AssignmentId == assignment.Id && s.StudentId == Guid.Parse(studentId), cancellationToken);

                        var today = DateOnly.FromDateTime(DateTime.Now); // Dapatkan tanggal hari ini tanpa informasi waktu
                        var deadline = assignment.AssignmentDeadline; // Deadline dari tugas

                        var status = submission == null
                            ? today > deadline ? "Kamu Terlambat" : "Belum Dikerjakan"
                            : submission.Grade.HasValue ? "Sudah Dinilai" : "Sudah Dikerjakan";

                        // Tambahkan status ke DTO
                        assignmentDto.AssignmentStatus = status;

                        assignmentDtos.Add(assignmentDto);
                    }

                    return Result<List<AssignmentGetByClassRoomIdDto>>.Success(assignmentDtos);
                }
                catch (Exception ex)
                {
                    // Tangani pengecualian dan kembalikan pesan kesalahan yang sesuai
                    return Result<List<AssignmentGetByClassRoomIdDto>>.Failure($"Failed to retrieve assignments: {ex.Message}");
                }
            }
        }
    }
}