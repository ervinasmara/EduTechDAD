using Application.Core;
using Application.Interface;
using Application.Submission;
using AutoMapper;
using Domain.Submission;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Infrastructure.Validation_Submission
{
    public class SubmissionService : ISubmissionService
    {
        private readonly DataContext _context;
        private readonly IUserAccessor _userAccessor;
        private readonly IMapper _mapper;
        private readonly IFileService _fileService;

        public SubmissionService(DataContext context, IUserAccessor userAccessor, IMapper mapper, IFileService fileService)
        {
            _context = context;
            _userAccessor = userAccessor;
            _mapper = mapper;
            _fileService = fileService;
        }

        public async Task<Result<AssignmentSubmission>> CreateSubmissionAsync(SubmissionCreateByStudentIdDto submissionDto, CancellationToken cancellationToken)
        {
            try
            {
                /** Langkah 1: Mendapatkan ID siswa dari token **/
                var studentId = Guid.Parse(_userAccessor.GetStudentIdFromToken());

                /** Langkah 2: Mengambil tugas yang sesuai dari database **/
                var assignment = await _context.Assignments.FindAsync(submissionDto.AssignmentId);
                if (assignment == null)
                    return Result<AssignmentSubmission>.Failure($"Assignment with ID {submissionDto.AssignmentId} not found.");

                /** Langkah 3: Memeriksa keberadaan siswa yang sesuai dengan ID dari token **/
                var student = await _context.Students.FindAsync(studentId);
                if (student == null)
                    return Result<AssignmentSubmission>.Failure($"Student with ID {studentId} not found.");

                /** Langkah 4: Memeriksa apakah Submission sudah ada sebelumnya **/
                var existingSubmission = await _context.AssignmentSubmissions
                    .FirstOrDefaultAsync(s => s.AssignmentId == assignment.Id && s.StudentId == studentId, cancellationToken);
                if (existingSubmission != null)
                    return Result<AssignmentSubmission>.Failure($"Submission already exists for assignment ID {assignment.Id} and student ID {studentId}.");

                /** Langkah 5: Membuat objek Submission tugas **/
                var assignmentSubmission = _mapper.Map<AssignmentSubmission>(submissionDto);
                assignmentSubmission.StudentId = studentId;

                /** Langkah 5.1: Menyimpan file Submission jika ada **/
                if (submissionDto.FileData != null)
                {
                    string relativeFolderPath = "Upload/FileAssignmentSubmission";
                    assignmentSubmission.FilePath = await _fileService.SaveFileSubmission(submissionDto.FileData, relativeFolderPath, DateTime.UtcNow);
                }

                /** Langkah 6: Menetapkan ID tugas dan menambahkan Submission ke konteks **/
                assignmentSubmission.AssignmentId = assignment.Id;
                _context.AssignmentSubmissions.Add(assignmentSubmission);

                /** Langkah 7: Menyimpan perubahan ke database **/
                var result = await _context.SaveChangesAsync(cancellationToken) > 0;
                if (!result)
                    return Result<AssignmentSubmission>.Failure("Failed to create assignment submission");

                /** Langkah 8: Mengembalikan hasil yang berhasil **/
                return Result<AssignmentSubmission>.Success(assignmentSubmission);
            }
            catch (Exception ex)
            {
                /** Langkah 9: Mengatasi kesalahan jika terjadi **/
                return Result<AssignmentSubmission>.Failure($"Failed to create assignment submission: {ex.Message}");
            }
        }
    }
}
