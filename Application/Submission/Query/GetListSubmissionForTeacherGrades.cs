using Application.Core;
using Application.Interface;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Submission.Query
{
    public class GetListSubmissionForTeacherGrades
    {
        public class Query : IRequest<Result<AssignmentSubmissionListForTeacherGradeDto>>
        {
            public Guid LessonId { get; set; }
            public Guid AssignmentId { get; set; }
        }

        public class Handler : IRequestHandler<Query, Result<AssignmentSubmissionListForTeacherGradeDto>>
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

            public async Task<Result<AssignmentSubmissionListForTeacherGradeDto>> Handle(Query request, CancellationToken cancellationToken)
            {
                var teacherId = Guid.Parse(_userAccessor.GetTeacherIdFromToken());

                // Verify the teacher is related to the lesson via TeacherLesson
                var lesson = await _context.Lessons
                    .Include(l => l.TeacherLessons)
                    .SingleOrDefaultAsync(l => l.Id == request.LessonId && l.TeacherLessons.Any(tl => tl.TeacherId == teacherId), cancellationToken);

                if (lesson == null)
                {
                    return Result<AssignmentSubmissionListForTeacherGradeDto>.Failure("Lesson not found or not related to the teacher.");
                }

                // Verify the assignment is related to the lesson
                var assignment = await _context.Assignments
                    .Include(c => c.Course)
                    .SingleOrDefaultAsync(a => a.Id == request.AssignmentId && a.Course.LessonId == request.LessonId, cancellationToken);

                if (assignment == null)
                {
                    return Result<AssignmentSubmissionListForTeacherGradeDto>.Failure("Assignment not found or not related to the lesson.");
                }

                // Get the list of AssignmentSubmissions for the Assignment
                var submissions = await _context.AssignmentSubmissions
                    .Where(s => s.AssignmentId == request.AssignmentId)
                    .ProjectTo<AssignmentSubmissionListGradeDto>(_mapper.ConfigurationProvider)
                    .ToListAsync(cancellationToken);

                // Calculate the counts for AlreadyGrades, NotAlreadyGrades, and NotYetSubmit
                var alreadyGradesCount = submissions.Count(s => s.Grade > 0);
                var notAlreadyGradesCount = submissions.Count(s => s.Grade == 0);
                var totalStudents = await _context.Students.CountAsync(s => s.ClassRoom.Lessons.Any(l => l.Id == request.LessonId), cancellationToken);
                var notYetSubmitCount = totalStudents - submissions.Count;

                // Dapatkan semua student yang terdaftar dalam ClassRoom yang terkait dengan Lesson
                var studentsInClass = await _context.Students
                    .Include(s => s.ClassRoom)
                    .Where(s => s.ClassRoom.Lessons.Any(l => l.Id == request.LessonId))
                    .ToListAsync(cancellationToken);

                // Dapatkan semua student yang sudah mengumpulkan tugas
                var studentsWhoSubmitted = await _context.AssignmentSubmissions
                    .Where(s => s.AssignmentId == request.AssignmentId)
                    .Select(s => s.StudentId)
                    .ToListAsync(cancellationToken);

                // Dapatkan semua student yang seharusnya submit tetapi belum submit
                var expectedSubmissions = studentsInClass
                    .Where(s => !studentsWhoSubmitted.Contains(s.Id))
                    .ToList();

                // Dapatkan semua student yang seharusnya submit tetapi belum submit
                var notYetSubmit = studentsInClass
                    .Where(s => !studentsWhoSubmitted.Contains(s.Id))
                    .Select(_mapper.Map<NotSubmittedDto>)
                    .ToList();

                // Create the result DTO
                var resultDto = new AssignmentSubmissionListForTeacherGradeDto
                {
                    AlreadyGrades = alreadyGradesCount.ToString(),
                    NotAlreadyGrades = notAlreadyGradesCount.ToString(),
                    NotYetSubmit = notYetSubmitCount.ToString(),
                    AssignmentSubmissionList = submissions,
                    StudentNotYetSubmit = notYetSubmit
                };

                return Result<AssignmentSubmissionListForTeacherGradeDto>.Success(resultDto);
            }
        }
    }
}
