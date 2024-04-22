using Application.Core;
using AutoMapper;
using FluentValidation;
using MediatR;
using Persistence;

namespace Application.Submission
{
    public class UpdateTeacher
    {
        public class Command : IRequest<Result<AssignmentSubmissionTeacherDto>>
        {
            public Guid Id { get; set; } // ID AssignmentSubmission
            public AssignmentSubmissionTeacherDto AssignmentSubmissionTeacherDto { get; set; }
        }

        public class CommandValidator : AbstractValidator<Command>
        {
            public CommandValidator()
            {
                RuleFor(x => x.AssignmentSubmissionTeacherDto.Grade).NotEmpty();
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
                    // Cari AssignmentSubmission berdasarkan ID
                    var assignmentSubmission = await _context.AssignmentSubmissions.FindAsync(request.Id);

                    if (assignmentSubmission == null)
                        return Result<AssignmentSubmissionTeacherDto>.Failure($"AssignmentSubmission with ID {request.Id} not found");

                    // Update nilai dan komentar
                    assignmentSubmission.Grade = request.AssignmentSubmissionTeacherDto.Grade;
                    assignmentSubmission.Comment = request.AssignmentSubmissionTeacherDto.Comment;

                    await _context.SaveChangesAsync(cancellationToken);

                    // Memetakan entitas AssignmentSubmission ke AssignmentSubmissionTeacherDto
                    var updatedDto = _mapper.Map<AssignmentSubmissionTeacherDto>(assignmentSubmission);

                    return Result<AssignmentSubmissionTeacherDto>.Success(updatedDto);
                }
                catch (Exception ex)
                {
                    return Result<AssignmentSubmissionTeacherDto>.Failure($"Failed to update AssignmentSubmission: {ex.Message}");
                }
            }
        }
    }
}