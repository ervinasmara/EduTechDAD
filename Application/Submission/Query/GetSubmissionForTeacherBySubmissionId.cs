using Application.Core;
using Application.Interface;
using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Submission.Query;
public class GetSubmissionForTeacherBySubmissionId
{
    public class Query : IRequest<Result<AssignmentSubmissionGetBySubmissionIdAndTeacherId>>
    {
        public Guid SubmissionId { get; set; }
    }

    public class Handler : IRequestHandler<Query, Result<AssignmentSubmissionGetBySubmissionIdAndTeacherId>>
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

        public async Task<Result<AssignmentSubmissionGetBySubmissionIdAndTeacherId>> Handle(Query request, CancellationToken cancellationToken)
        {
            try
            {
                /** Langkah 1: Mendapatkan ID guru dari token **/
                var teacherIdFromToken = Guid.Parse(_userAccessor.GetTeacherIdFromToken());

                /** Langkah 2: Memeriksa apakah ID guru ditemukan di token **/
                if (teacherIdFromToken == Guid.Empty)
                    return Result<AssignmentSubmissionGetBySubmissionIdAndTeacherId>.Failure("TeacherId tidak ditemukan ditoken.");

                /** Langkah 3: Mendapatkan pengajuan tugas dengan menyertakan informasi yang terkait **/
                var assignmentSubmission = await _context.AssignmentSubmissions
                    .Include(s => s.Student)
                    .Include(s => s.Assignment)
                        .ThenInclude(a => a.Course)
                            .ThenInclude(c => c.Lesson)
                                .ThenInclude(l => l.TeacherLessons)
                    .Where(s => s.Id == request.SubmissionId)
                    .SingleOrDefaultAsync(cancellationToken);

                /** Langkah 4: Memeriksa apakah pengajuan tugas ditemukan **/
                if (assignmentSubmission == null)
                    return Result<AssignmentSubmissionGetBySubmissionIdAndTeacherId>.Failure("Pengumpulan tugas tidak ditemukan");

                /** Langkah 5: Memeriksa apakah guru terkait dengan pelajaran **/
                var isTeacherRelatedToLesson = assignmentSubmission.Assignment.Course.Lesson.TeacherLessons
                    .Any(tl => tl.TeacherId == teacherIdFromToken);

                /** Langkah 6: Memeriksa apakah pengajuan tugas terkait dengan guru **/
                if (!isTeacherRelatedToLesson)
                    return Result<AssignmentSubmissionGetBySubmissionIdAndTeacherId>.Failure("Pengumpulan tugas tersebut tidak terkait dengan guru.");

                /** Langkah 7: Mengonversi entity pengajuan tugas menjadi DTO **/
                var assignmentSubmissionDto = _mapper.Map<AssignmentSubmissionGetBySubmissionIdAndTeacherId>(assignmentSubmission);

                /** Langkah 8: Mengembalikan pengajuan tugas yang berhasil ditemukan **/
                return Result<AssignmentSubmissionGetBySubmissionIdAndTeacherId>.Success(assignmentSubmissionDto);
            }
            catch (Exception ex)
            {
                /** Langkah 9: Menangani kesalahan jika terjadi **/
                return Result<AssignmentSubmissionGetBySubmissionIdAndTeacherId>.Failure($"Gagal menangani pengumpulan tugas: {ex.Message}");
            }
        }
    }
}