using Application.Core;
using Application.Learn.GetFileName;
using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Assignments
{
    public class ListTasks
    {
        public class Query : IRequest<Result<List<AssignmentGetDto>>>
        {
            // Tidak memerlukan parameter tambahan untuk meneruskan ke query
        }

        public class Handler : IRequestHandler<Query, Result<List<AssignmentGetDto>>>
        {
            private readonly DataContext _context;

            public Handler(DataContext context)
            {
                _context = context;
            }

            public async Task<Result<List<AssignmentGetDto>>> Handle(Query request, CancellationToken cancellationToken)
            {
                try
                {
                    // Dapatkan semua tugas (assignments)
                    var assignments = await _context.Assignments
                        .Include(a => a.Course)
                            .ThenInclude(c => c.Lesson) // Termasuk informasi Lesson dalam query
                        .ToListAsync(cancellationToken);

                    var assignmentDtos = new List<AssignmentGetDto>();

                    foreach (var assignment in assignments)
                    {
                        // Dapatkan nama pelajaran dari tugas yang terkait
                        var lessonName = assignment.Course?.Lesson?.LessonName;

                        // Dapatkan nama-nama kelas yang terkait dengan assignment
                        var classNames = await GetClassNamesForAssignment(assignment.Id, cancellationToken);

                        // Tentukan status deadline
                        var assignmentStatusDeadline = GetAssignmentStatusDeadline(assignment.AssignmentDeadline);

                        // Buat DTO AssignmentGetDto
                        var assignmentDto = new AssignmentGetDto
                        {
                            Id = assignment.Id,
                            AssignmentName = assignment.AssignmentName,
                            AssignmentDate = new DateOnly(assignment.AssignmentDate.Year, assignment.AssignmentDate.Month, assignment.AssignmentDate.Day), // Konversi ke DateOnly
                            AssignmentDeadline = new DateOnly(assignment.AssignmentDeadline.Year, assignment.AssignmentDeadline.Month, assignment.AssignmentDeadline.Day), // Konversi ke DateOnly
                            AssignmentDescription = assignment.AssignmentDescription,
                            AssignmentLink = assignment.AssignmentLink,
                            LessonName = lessonName,
                            AssignmentFileData = assignment.FileData,
                            ClassNames = classNames,
                            AssignmentStatus = assignmentStatusDeadline
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

                        assignmentDtos.Add(assignmentDto);
                    }

                    return Result<List<AssignmentGetDto>>.Success(assignmentDtos);
                }
                catch (Exception ex)
                {
                    // Tangani pengecualian dan kembalikan pesan kesalahan yang sesuai
                    return Result<List<AssignmentGetDto>>.Failure($"Failed to retrieve assignments: {ex.Message}");
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

            private string GetAssignmentStatusDeadline(DateOnly assignmentDeadline)
            {
                var today = DateOnly.FromDateTime(DateTime.Today);
                var tomorrow = today.AddDays(1);

                if (assignmentDeadline >= today && assignmentDeadline < tomorrow)
                {
                    return "Belum Dikerjakan";
                }
                else if (assignmentDeadline < today)
                {
                    return "Kamu Terlambat";
                }
                else
                {
                    return "Sesuai Waktu";
                }
            }
        }
    }
}