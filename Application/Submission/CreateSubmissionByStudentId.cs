using MediatR;
using Persistence;
using Application.Core;
using Application.Interface;
using Domain.Submission;
using AutoMapper;
using Microsoft.EntityFrameworkCore;

namespace Application.Submission
{
    public class CreateSubmission
    {
        public class Command : IRequest<Result<SubmissionCreateDto>>
        {
            public SubmissionCreateDto SubmissionDto { get; set; }
        }

        //public class CommandValidator : AbstractValidator<Command>
        //{
        //    public CommandValidator()
        //    {
        //        RuleFor(x => x.SubmissionDto).SetValidator(new SubmissionCreateValidator());
        //    }
        //}

        public class Handler : IRequestHandler<Command, Result<SubmissionCreateDto>>
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

            public async Task<Result<SubmissionCreateDto>> Handle(Command request, CancellationToken cancellationToken)
            {
                try
                {
                    // Retrieve StudentId from token
                    var studentId = Guid.Parse(_userAccessor.GetStudentIdFromToken()); // Assuming a method to retrieve current user's id from token

                    // Check if the assignment exists
                    var assignment = await _context.Assignments.FindAsync(request.SubmissionDto.AssignmentId);
                    if (assignment == null)
                        return Result<SubmissionCreateDto>.Failure($"Assignment with ID {request.SubmissionDto.AssignmentId} not found.");

                    // Check if the student exists
                    var student = await _context.Students.FindAsync(studentId);
                    if (student == null)
                        return Result<SubmissionCreateDto>.Failure($"Student with ID {studentId} not found.");

                    // Check if there's an existing submission
                    var existingSubmission = await _context.AssignmentSubmissions
                        .FirstOrDefaultAsync(s => s.AssignmentId == assignment.Id && s.StudentId == studentId);

                    if (existingSubmission != null)
                        return Result<SubmissionCreateDto>.Failure($"Assignment submission for assignment ID {assignment.Id} and student ID {studentId} already exists.");

                    // Create new submission
                    var assignmentSubmission = new AssignmentSubmission
                    {
                        AssignmentId = assignment.Id,
                        StudentId = studentId,
                        SubmissionTime = DateTime.UtcNow.AddHours(7)
                    };

                    // Fill in other properties if provided
                    if (request.SubmissionDto.FileData != null)
                    {
                        using (var memoryStream = new MemoryStream())
                        {
                            await request.SubmissionDto.FileData.CopyToAsync(memoryStream);
                            assignmentSubmission.FileData = memoryStream.ToArray();
                        }
                    }
                    assignmentSubmission.Link = request.SubmissionDto.Link;

                    // Add and save to database
                    _context.AssignmentSubmissions.Add(assignmentSubmission);
                    await _context.SaveChangesAsync(cancellationToken);

                    // Map to DTO
                    var submissionDto = _mapper.Map<SubmissionCreateDto>(assignmentSubmission);
                    return Result<SubmissionCreateDto>.Success(submissionDto);
                }
                catch (Exception ex)
                {
                    // Handle exception
                    return Result<SubmissionCreateDto>.Failure($"Failed to create assignment submission: {ex.Message}");
                }
            }
        }
    }

}