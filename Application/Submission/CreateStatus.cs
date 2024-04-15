using Application.Core;
using Application.Attendances;
using AutoMapper;
using Domain.Submission;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Submission
{
    public class CreateStatus
    {
        public class Command : IRequest<Result<AssignmentSubmissionStatusDto>>
        {
            public AssignmentSubmissionStatusDto AssignmentSubmissionStatusDto { get; set; }
        }

        public class CommandValidator : AbstractValidator<Command>
        {
            public CommandValidator()
            {
                RuleFor(x => x.AssignmentSubmissionStatusDto).SetValidator(new AssignmentSubmissionValidator());
            }
        }

        public class Handler : IRequestHandler<Command, Result<AssignmentSubmissionStatusDto>>
        {
            private readonly DataContext _context;
            private readonly IMapper _mapper;

            public Handler(DataContext context, IMapper mapper)
            {
                _context = context;
                _mapper = mapper;
            }

            public async Task<Result<AssignmentSubmissionStatusDto>> Handle(Command request, CancellationToken cancellationToken)
            {
                try
                {
                    // Cek apakah assignment dengan ID yang diberikan ada
                    var assignment = await _context.Assignments.FindAsync(request.AssignmentSubmissionStatusDto.AssignmentId);
                    if (assignment == null)
                        return Result<AssignmentSubmissionStatusDto>.Failure($"Assignment with ID {request.AssignmentSubmissionStatusDto.AssignmentId} not found.");

                    // Cek apakah student dengan ID yang diberikan ada
                    var student = await _context.Students.FindAsync(request.AssignmentSubmissionStatusDto.StudentId);
                    if (student == null)
                        return Result<AssignmentSubmissionStatusDto>.Failure($"Student with ID {request.AssignmentSubmissionStatusDto.StudentId} not found.");

                    // Cek apakah sudah ada AssignmentSubmission dengan AssignmentId dan StudentId yang sama
                    var existingSubmission = await _context.AssignmentSubmissions
                        .FirstOrDefaultAsync(s => s.AssignmentId == assignment.Id && s.StudentId == request.AssignmentSubmissionStatusDto.StudentId);

                    if (existingSubmission != null)
                        return Result<AssignmentSubmissionStatusDto>.Failure($"Assignment submission for assignment ID {assignment.Id} and student ID {request.AssignmentSubmissionStatusDto.StudentId} already exists.");

                    // Buat entitas AssignmentSubmission dengan status default = 1
                    var assignmentSubmission = new AssignmentSubmission
                    {
                        AssignmentId = assignment.Id,
                        StudentId = request.AssignmentSubmissionStatusDto.StudentId,
                        Status = 1 // Set nilai default status
                    };

                    // Tambah AssignmentSubmission ke database
                    _context.AssignmentSubmissions.Add(assignmentSubmission);
                    await _context.SaveChangesAsync(cancellationToken);

                    // Memetakan entitas AssignmentSubmission ke AssignmentSubmissionStatusDto
                    var assignmentSubmissionDto = _mapper.Map<AssignmentSubmissionStatusDto>(assignmentSubmission);
                    return Result<AssignmentSubmissionStatusDto>.Success(assignmentSubmissionDto);
                }
                catch (Exception ex)
                {
                    // Menangani pengecualian dan mengembalikan pesan kesalahan yang sesuai
                    return Result<AssignmentSubmissionStatusDto>.Failure($"Failed to create assignment submission: {ex.Message}");
                }
            }
        }
    }
}
