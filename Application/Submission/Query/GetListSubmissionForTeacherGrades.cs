using Application.Core;
using Application.Interface;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Submission.Query;
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
            try
            {
                /** Langkah 1: Mendapatkan ID guru dari token **/
                var teacherId = Guid.Parse(_userAccessor.GetTeacherIdFromToken());

                /** Langkah 2: Memverifikasi apakah guru terkait dengan pelajaran melalui TeacherLesson **/
                var lesson = await _context.Lessons
                    .Include(l => l.TeacherLessons)
                    .SingleOrDefaultAsync(l => l.Id == request.LessonId && l.TeacherLessons.Any(tl => tl.TeacherId == teacherId), cancellationToken);

                /** Langkah 3: Memeriksa apakah pelajaran ditemukan dan terkait dengan guru **/
                if (lesson == null)
                {
                    return Result<AssignmentSubmissionListForTeacherGradeDto>.Failure("Pelajaran tidak ditemukan atau tidak berhubungan dengan guru");
                }

                /** Langkah 4: Memverifikasi apakah tugas terkait dengan pelajaran **/
                var assignment = await _context.Assignments
                    .Include(c => c.Course)
                    .SingleOrDefaultAsync(a => a.Id == request.AssignmentId && a.Course.LessonId == request.LessonId, cancellationToken);

                /** Langkah 5: Memeriksa apakah tugas ditemukan dan terkait dengan pelajaran **/
                if (assignment == null)
                {
                    return Result<AssignmentSubmissionListForTeacherGradeDto>.Failure("Tugas yang tidak ditemukan atau tidak berhubungan dengan pelajaran");
                }

                /** Langkah 6: Mendapatkan daftar pengajuan tugas untuk tugas yang diberikan **/
                var submissions = await _context.AssignmentSubmissions
                    .Where(s => s.AssignmentId == request.AssignmentId)
                    .ProjectTo<AssignmentSubmissionListGradeDto>(_mapper.ConfigurationProvider)
                    .OrderBy(a => a.SubmissionTime)
                    .ToListAsync(cancellationToken);

                /** Langkah 7: Menghitung jumlah Pengajuan yang sudah dinilai, belum dinilai, dan belum dikumpulkan **/
                var alreadyGradesCount = submissions.Count(s => s.Grade > 0);
                var notAlreadyGradesCount = submissions.Count(s => s.Grade == 0);
                var totalStudents = await _context.Students.CountAsync(s => s.ClassRoom.Lessons.Any(l => l.Id == request.LessonId), cancellationToken);
                var notYetSubmitCount = totalStudents - submissions.Count;

                /** Langkah 8: Mendapatkan daftar siswa yang terdaftar dalam kelas yang terkait dengan pelajaran **/
                var studentsInClass = await _context.Students
                    .Include(s => s.ClassRoom)
                    .Where(s => s.ClassRoom.Lessons.Any(l => l.Id == request.LessonId))
                    .ToListAsync(cancellationToken);

                /** Langkah 9: Mendapatkan daftar siswa yang sudah mengumpulkan tugas **/
                var studentsWhoSubmitted = await _context.AssignmentSubmissions
                    .Where(s => s.AssignmentId == request.AssignmentId)
                    .Select(s => s.StudentId)
                    .ToListAsync(cancellationToken);

                /** Langkah 10: Mendapatkan daftar siswa yang seharusnya mengumpulkan tugas tetapi belum mengumpulkan **/
                var notYetSubmit = studentsInClass
                    .Where(s => !studentsWhoSubmitted.Contains(s.Id))
                    .Select(_mapper.Map<NotSubmittedDto>)
                    .ToList();

                /** Langkah 11: Membuat DTO hasil **/
                var resultDto = new AssignmentSubmissionListForTeacherGradeDto
                {
                    AlreadyGrades = alreadyGradesCount.ToString(),
                    NotAlreadyGrades = notAlreadyGradesCount.ToString(),
                    NotYetSubmit = notYetSubmitCount.ToString(),
                    AssignmentSubmissionList = submissions,
                    StudentNotYetSubmit = notYetSubmit
                };

                /** Langkah 12: Mengembalikan hasil yang berhasil **/
                return Result<AssignmentSubmissionListForTeacherGradeDto>.Success(resultDto);
            }
            catch (Exception ex)
            {
                /** Langkah 13: Menangani kesalahan jika terjadi **/
                return Result<AssignmentSubmissionListForTeacherGradeDto>.Failure($"Gagal menangani pengumpulan tugas: {ex.Message}");
            }
        }

    }
}