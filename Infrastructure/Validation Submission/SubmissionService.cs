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
                var studentId = Guid.Parse(_userAccessor.GetStudentIdFromToken());

                var assignment = await _context.Assignments.FindAsync(submissionDto.AssignmentId);
                if (assignment == null)
                    return Result<AssignmentSubmission>.Failure($"Assignment with ID {submissionDto.AssignmentId} not found.");

                var student = await _context.Students.FindAsync(studentId);
                if (student == null)
                    return Result<AssignmentSubmission>.Failure($"Student with ID {studentId} not found.");

                var existingSubmission = await _context.AssignmentSubmissions
                    .FirstOrDefaultAsync(s => s.AssignmentId == assignment.Id && s.StudentId == studentId, cancellationToken);
                if (existingSubmission != null)
                    return Result<AssignmentSubmission>.Failure($"Submission already exists for assignment ID {assignment.Id} and student ID {studentId}.");

                var assignmentSubmission = _mapper.Map<AssignmentSubmission>(submissionDto);
                assignmentSubmission.StudentId = studentId;

                if (submissionDto.FileData != null)
                {
                    string relativeFolderPath = "Upload/FileAssignmentSubmission";
                    assignmentSubmission.FilePath = await _fileService.SaveFileSubmission(submissionDto.FileData, relativeFolderPath, DateTime.UtcNow);
                }

                assignmentSubmission.AssignmentId = assignment.Id;

                _context.AssignmentSubmissions.Add(assignmentSubmission);
                var result = await _context.SaveChangesAsync(cancellationToken) > 0;
                if (!result)
                    return Result<AssignmentSubmission>.Failure("Failed to create assignment submission");

                return Result<AssignmentSubmission>.Success(assignmentSubmission);
            }
            catch (Exception ex)
            {
                return Result<AssignmentSubmission>.Failure($"Failed to create assignment submission: {ex.Message}");
            }
        }

        public async Task<Result<AssignmentSubmission>> EditSubmissionByStudentIdAsync(SubmissionEditByStudentIdDto editDto, Guid submissionId, CancellationToken cancellationToken)
        {
            var studentIdFromToken = _userAccessor.GetStudentIdFromToken();
            if (string.IsNullOrEmpty(studentIdFromToken))
                return Result<AssignmentSubmission>.Failure("Student ID not found in token.");

            var studentId = Guid.Parse(studentIdFromToken);
            var submission = await _context.AssignmentSubmissions
                .Include(s => s.Student)
                .FirstOrDefaultAsync(s => s.Id == submissionId, cancellationToken);

            if (submission == null)
                return Result<AssignmentSubmission>.Failure($"Submission with ID {submissionId} not found.");
            if (submission.StudentId != studentId)
                return Result<AssignmentSubmission>.Failure("You can only edit your own submissions.");

            // Use AutoMapper to map the DTO to the submission entity
            _mapper.Map(editDto, submission);

            // Handle file separately if provided
            if (editDto.FileData != null)
            {
                string relativeFolderPath = "Upload/FileAssignmentSubmission";
                submission.FilePath = await _fileService.SaveFileSubmission(editDto.FileData, relativeFolderPath, DateTime.UtcNow);
            }

            _context.AssignmentSubmissions.Update(submission);
            var result = await _context.SaveChangesAsync(cancellationToken) > 0;
            if (!result)
                return Result<AssignmentSubmission>.Failure("Failed to edit assignment submission");

            return Result<AssignmentSubmission>.Success(submission);
        }
    }
}
