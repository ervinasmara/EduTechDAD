using MediatR;
using Persistence;
using FluentValidation;
using Application.Core;
using Microsoft.EntityFrameworkCore;
using Domain.Task;
using AutoMapper;

namespace Application.Tasks
{
    public class EditTask
    {
        public class Command : IRequest<Result<AssignmentDto>>
        {
            public Guid Id { get; set; }
            public AssignmentDto AssignmentDto { get; set; }
        }

        public class CommandValidatorDto : AbstractValidator<Command>
        {
            public CommandValidatorDto()
            {
                RuleFor(x => x.AssignmentDto).SetValidator(new AssignmentValidator());
            }
        }

        public class Handler : IRequestHandler<Command, Result<AssignmentDto>>
        {
            private readonly DataContext _context;
            private readonly IMapper _mapper;

            public Handler(DataContext context, IMapper mapper)
            {
                _context = context;
                _mapper = mapper;
            }

            public async Task<Result<AssignmentDto>> Handle(Command request, CancellationToken cancellationToken)
            {
                try
                {
                    var assignment = await _context.Assignments.FindAsync(request.Id);

                    if (assignment == null)
                        return Result<AssignmentDto>.Failure("Assignment not found.");

                    // Check if Course with provided CourseId exists
                    var course = await _context.Courses.FindAsync(request.AssignmentDto.CourseId);
                    if (course == null)
                        return Result<AssignmentDto>.Failure($"Course with ID {request.AssignmentDto.CourseId} not found.");

                    // Update assignment properties
                    assignment.AssignmentName = request.AssignmentDto.AssignmentName;
                    assignment.AssignmentDate = request.AssignmentDto.AssignmentDate;
                    assignment.AssignmentDeadline = request.AssignmentDto.AssignmentDeadline;
                    assignment.AssignmentDescription = request.AssignmentDto.AssignmentDescription;
                    assignment.AssignmentLink = request.AssignmentDto.AssignmentLink;
                    assignment.CourseId = request.AssignmentDto.CourseId;

                    // Read file data and update FileData if provided
                    if (request.AssignmentDto.AssignmentFileData != null)
                    {
                        using (var memoryStream = new MemoryStream())
                        {
                            await request.AssignmentDto.AssignmentFileData.CopyToAsync(memoryStream);
                            assignment.FileData = memoryStream.ToArray();
                        }
                    }

                    _context.Entry(assignment).State = EntityState.Modified;

                    var success = await _context.SaveChangesAsync(cancellationToken) > 0;

                    if (!success)
                        return Result<AssignmentDto>.Failure("Failed to edit assignment.");

                    var editedAssignmentDto = _mapper.Map<AssignmentDto>(assignment);

                    return Result<AssignmentDto>.Success(editedAssignmentDto);
                }
                catch (Exception ex)
                {
                    return Result<AssignmentDto>.Failure($"Failed to edit assignment: {ex.Message}");
                }
            }
        }
    }
}