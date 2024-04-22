using Application.Core;
using Application.Interface;
using Application.Learn.GetFileName;
using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Assignments
{
    public class ListAssignmentByClassRoomId
    {
        public class Query : IRequest<Result<List<AssignmentGetByTeacherIdDto>>>
        {
            // Tidak memerlukan parameter tambahan untuk meneruskan ke query
        }

        public class Handler : IRequestHandler<Query, Result<List<AssignmentGetByTeacherIdDto>>>
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

            public async Task<Result<List<AssignmentGetByTeacherIdDto>>> Handle(Query request, CancellationToken cancellationToken)
            {
                var classRoomId = _userAccessor.GetClassRoomIdFromToken();

                // Periksa apakah classRoomId valid
                if (string.IsNullOrEmpty(classRoomId))
                {
                    return Result<List<AssignmentGetByTeacherIdDto>>.Failure("ClassRoomId not found in token.");
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
                        .ToListAsync(cancellationToken);

                    var assignmentDtos = new List<AssignmentGetByTeacherIdDto>();

                    foreach (var assignment in assignments)
                    {
                        // Dapatkan nama pelajaran dari tugas yang terkait
                        var lessonName = assignment.Course.Lesson.LessonName;

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
                            assignmentDto.AssignmentFileName = "UnknownFileName";
                        }

                        assignmentDtos.Add(assignmentDto);
                    }

                    return Result<List<AssignmentGetByTeacherIdDto>>.Success(assignmentDtos);
                }
                catch (Exception ex)
                {
                    // Tangani pengecualian dan kembalikan pesan kesalahan yang sesuai
                    return Result<List<AssignmentGetByTeacherIdDto>>.Failure($"Failed to retrieve assignments: {ex.Message}");
                }
            }
        }
    }
}
