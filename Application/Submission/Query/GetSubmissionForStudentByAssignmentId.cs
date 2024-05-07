using Application.Core;
using Application.Interface;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Submission.Query;
public class GetSubmissionForStudentByAssignmentId
{
    public class Query : IRequest<Result<AssignmentSubmissionGetByAssignmentIdAndStudentId>>
    {
        public Guid AssignmentId { get; set; }
    }

    public class Handler : IRequestHandler<Query, Result<AssignmentSubmissionGetByAssignmentIdAndStudentId>>
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

        public async Task<Result<AssignmentSubmissionGetByAssignmentIdAndStudentId>> Handle(Query request, CancellationToken cancellationToken)
        {
            try
            {
                /** Langkah 1: Mendapatkan ID siswa dari token **/
                var studentIdFromToken = Guid.Parse(_userAccessor.GetStudentIdFromToken());

                /** Langkah 2: Memeriksa apakah ID siswa ditemukan di token **/
                if (studentIdFromToken == Guid.Empty)
                    return Result<AssignmentSubmissionGetByAssignmentIdAndStudentId>.Failure("StudentId tidak ditemukan ditoken.");

                /** Langkah 3: Mencari pengajuan tugas berdasarkan ID tugas dan ID siswa **/
                var assignmentSubmissionDto = await _context.AssignmentSubmissions
                    .Where(s => s.AssignmentId == request.AssignmentId && s.StudentId == studentIdFromToken)
                    .ProjectTo<AssignmentSubmissionGetByAssignmentIdAndStudentId>(_mapper.ConfigurationProvider)
                    .SingleOrDefaultAsync(cancellationToken);

                /** Langkah 4: Memeriksa apakah pengajuan tugas ditemukan **/
                if (assignmentSubmissionDto == null)
                    return Result<AssignmentSubmissionGetByAssignmentIdAndStudentId>.Failure("Pengumpulan tugas tidak ditemukan");

                /** Langkah 5: Mengembalikan pengajuan tugas yang berhasil ditemukan **/
                return Result<AssignmentSubmissionGetByAssignmentIdAndStudentId>.Success(assignmentSubmissionDto);
            }
            catch (Exception ex)
            {
                /** Langkah 6: Menangani kesalahan jika terjadi **/
                return Result<AssignmentSubmissionGetByAssignmentIdAndStudentId>.Failure($"Gagal menangani pengumpulan tugas: {ex.Message}");
            }
        }
    }
}