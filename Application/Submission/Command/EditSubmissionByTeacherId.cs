using Application.Core;
using AutoMapper;
using FluentValidation;
using MediatR;
using Persistence;

namespace Application.Submission.Command
{
    public class EditSubmissionByTeacherId
    {
        public class Command : IRequest<Result<AssignmentSubmissionTeacherDto>>
        {
            public Guid SubmissionId { get; set; } // ID AssignmentSubmission
            public AssignmentSubmissionTeacherDto AssignmentSubmissionTeacherDto { get; set; }
        }

        public class CommandValidator : AbstractValidator<Command>
        {
            public CommandValidator()
            {
                RuleFor(x => x.AssignmentSubmissionTeacherDto.Grade)
                  .NotEmpty()
                  .InclusiveBetween(0.0f, 100.0f)
                  .WithMessage("Grade must be a number between 0 and 100");
                RuleFor(x => x.AssignmentSubmissionTeacherDto.Comment).NotEmpty();
            }
        }

        public class Handler : IRequestHandler<Command, Result<AssignmentSubmissionTeacherDto>>
        {
            private readonly DataContext _context;
            private readonly IMapper _mapper;

            public Handler(DataContext context, IMapper mapper)
            {
                _context = context;
                _mapper = mapper;
            }

            public async Task<Result<AssignmentSubmissionTeacherDto>> Handle(Command request, CancellationToken cancellationToken)
            {
                try
                {
                    // Find AssignmentSubmission
                    var assignmentSubmission = await _context.AssignmentSubmissions.FindAsync(request.SubmissionId);

                    if (assignmentSubmission == null)
                    {
                        return Result<AssignmentSubmissionTeacherDto>.Failure($"AssignmentSubmission with ID {request.SubmissionId} not found");
                    }

                    // Update using AutoMapper
                    _mapper.Map(request.AssignmentSubmissionTeacherDto, assignmentSubmission);

                    await _context.SaveChangesAsync(cancellationToken);

                    return Result<AssignmentSubmissionTeacherDto>.Success(request.AssignmentSubmissionTeacherDto); // Mapped automatically
                }
                catch (Exception ex)
                {
                    return Result<AssignmentSubmissionTeacherDto>.Failure($"Failed to update AssignmentSubmission: {ex.Message}");
                }
            }
        }
    }
}