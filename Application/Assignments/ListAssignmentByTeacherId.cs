using Application.Core;
using Application.Interface;
using Application.Learn.GetFileName;
using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
                var teacherId = _userAccessor.GetTeacherIdFromToken();

                // Periksa apakah teacherId valid
                if (string.IsNullOrEmpty(teacherId))
                {
                    return Result<List<AssignmentGetByTeacherIdDto>>.Failure("TeacherId not found in token.");
                }

                try
                {
                    // Dapatkan semua LessonId yang terkait dengan TeacherId
                    var lessonIds = await _context.TeacherLessons
                        .Where(tl => tl.TeacherId == Guid.Parse(teacherId))
                        .Select(tl => tl.LessonId)
                        .ToListAsync(cancellationToken);

                    // Dapatkan semua CourseId yang terkait dengan TeacherId
                    var courseIds = await _context.TeacherCourses
                        .Where(tc => tc.TeacherId == Guid.Parse(teacherId))
                        .Select(tc => tc.CourseId)
                        .ToListAsync(cancellationToken);

                    // Dapatkan semua tugas yang terkait dengan CourseId yang ditemukan
                    var assignments = await _context.Assignments
                        .Include(a => a.Course)
                        .ThenInclude(c => c.Lesson) // Termasuk informasi Lesson dalam query
                        .Where(a => courseIds.Contains(a.CourseId))
                        .ToListAsync(cancellationToken);

                    // Filter tugas yang terkait dengan LessonId yang ditemukan
                    assignments = assignments.Where(a => a.Course != null && a.Course.Lesson != null && lessonIds.Contains(a.Course.Lesson.Id)).ToList();

                    var assignmentDtos = new List<AssignmentGetByTeacherIdDto>();

                    foreach (var assignment in assignments)
                    {
                        // Periksa null sebelum mengakses properti-properti assignment, Course, dan Lesson
                        if (assignment != null && assignment.Course != null && assignment.Course.Lesson != null)
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
