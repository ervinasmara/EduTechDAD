using MediatR;
using Application.Core;
using Application.Interface;
using FluentValidation;
using AutoMapper;
using Domain.Submission;
using Persistence;
using Microsoft.EntityFrameworkCore;

namespace Application.Submission.Command
{
    public class CreateSubmissionByStudentId
    {
        public class Command : IRequest<Result<SubmissionCreateByStudentIdDto>>
        {
            public SubmissionCreateByStudentIdDto SubmissionDto { get; set; }
        }

        public class CommandValidator : AbstractValidator<Command>
        {
            public CommandValidator()
            {
                RuleFor(x => x.SubmissionDto.AssignmentId).NotEmpty();
                RuleFor(x => x.SubmissionDto.Link)
                    .NotEmpty()
                    .When(x => x.SubmissionDto.FileData == null) // Hanya memeriksa Link jika FileData kosong
                    .WithMessage("AssignmentLink must be provided if FileData is not provided.");
            }
        }

        public class Handler : IRequestHandler<Command, Result<SubmissionCreateByStudentIdDto>>
        {
            private readonly DataContext _context;
            private readonly IUserAccessor _userAccessor;
            private readonly IMapper _mapper;
            private readonly IFileService _fileService;

            public Handler(DataContext context, IUserAccessor userAccessor, IMapper mapper, IFileService fileService)
            {
                _context = context;
                _userAccessor = userAccessor;
                _mapper = mapper;
                _fileService = fileService;
            }

            public async Task<Result<SubmissionCreateByStudentIdDto>> Handle(Command request, CancellationToken cancellationToken)
            {
                try
                {
                    /** Langkah 1: Ambil ID Siswa dari token **/
                    var studentId = Guid.Parse(_userAccessor.GetStudentIdFromToken());

                    /** Langkah 2: Ambil tugas yang sesuai dari database **/
                    var assignment = await _context.Assignments.FindAsync(request.SubmissionDto.AssignmentId);
                    if (assignment == null)
                        return Result<SubmissionCreateByStudentIdDto>.Failure($"Assignment with ID {request.SubmissionDto.AssignmentId} not found.");

                    /** Langkah 3: Ambil siswa yang sesuai dari database **/
                    var student = await _context.Students.FindAsync(studentId);
                    if (student == null)
                        return Result<SubmissionCreateByStudentIdDto>.Failure($"Student with ID {studentId} not found.");

                    /** Langkah 4: Periksa apakah submission sudah ada sebelumnya **/
                    var existingSubmission = await _context.AssignmentSubmissions
                        .FirstOrDefaultAsync(s => s.AssignmentId == assignment.Id && s.StudentId == studentId, cancellationToken);
                    if (existingSubmission != null)
                        return Result<SubmissionCreateByStudentIdDto>.Failure($"Submission already exists for assignment ID {assignment.Id} and student ID {studentId}.");

                    /** Langkah 5: Buat objek SubmissionCreateByStudentIdDto **/
                    var submissionDto = new SubmissionCreateByStudentIdDto
                    {
                        AssignmentId = request.SubmissionDto.AssignmentId,
                        FileData = request.SubmissionDto.FileData,
                        Link = request.SubmissionDto.Link
                    };

                    /** Langkah 5.1: Pemetaan dari DTO ke objek AssignmentSubmission **/
                    var assignmentSubmission = _mapper.Map<AssignmentSubmission>(submissionDto);
                    assignmentSubmission.StudentId = studentId;

                    /** Langkah 5.2: Simpan file submission jika ada **/
                    if (request.SubmissionDto.FileData != null)
                    {
                        string relativeFolderPath = "Upload/FileAssignmentSubmission";
                        assignmentSubmission.FilePath = await _fileService.SaveFileSubmission(request.SubmissionDto.FileData, relativeFolderPath, DateTime.UtcNow);
                    }

                    /** Langkah 5.3: Validasi jenis file **/
                    if (request.SubmissionDto.FileData != null)
                    {
                        string fileExtension = Path.GetExtension(request.SubmissionDto.FileData.FileName);
                        if (!string.Equals(fileExtension, ".pdf", StringComparison.OrdinalIgnoreCase))
                        {
                            return Result<SubmissionCreateByStudentIdDto>.Failure("Only PDF files are allowed.");
                        }
                    }

                    /** Langkah 6: Tetapkan ID tugas dan tambahkan submission ke konteks **/
                    assignmentSubmission.AssignmentId = assignment.Id;
                    _context.AssignmentSubmissions.Add(assignmentSubmission);

                    /** Langkah 7: Simpan perubahan ke database **/
                    var result = await _context.SaveChangesAsync(cancellationToken) > 0;
                    if (!result)
                        return Result<SubmissionCreateByStudentIdDto>.Failure("Failed to create assignment submission");

                    /** Langkah 8: Kembalikan hasil yang berhasil **/
                    return Result<SubmissionCreateByStudentIdDto>.Success(submissionDto);
                }
                catch (Exception ex)
                {
                    /** Langkah 9: Tangani kesalahan jika terjadi **/
                    return Result<SubmissionCreateByStudentIdDto>.Failure($"Failed to create assignment submission: {ex.Message}");
                }
            }
        }
    }
}