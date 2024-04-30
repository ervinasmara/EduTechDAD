using Application.Core;
using Application.Submission.Validator;
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
                RuleFor(x => x.AssignmentSubmissionTeacherDto).SetValidator(new SubmissionGradesForTeacherValidator());
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
                    /** Langkah 1: Mencari AssignmentSubmission berdasarkan ID yang diberikan **/
                    var assignmentSubmission = await _context.AssignmentSubmissions.FindAsync(request.SubmissionId);

                    /** Langkah 2: Memeriksa apakah AssignmentSubmission ditemukan **/
                    if (assignmentSubmission == null)
                    {
                        return Result<AssignmentSubmissionTeacherDto>.Failure($"AssignmentSubmission with ID {request.SubmissionId} not found");
                    }

                    /** Langkah 3: Menggunakan AutoMapper untuk memperbarui AssignmentSubmission **/
                    _mapper.Map(request.AssignmentSubmissionTeacherDto, assignmentSubmission);

                    /** Langkah 4: Menyimpan perubahan ke database **/
                    await _context.SaveChangesAsync(cancellationToken);

                    /** Langkah 5: Mengembalikan hasil yang berhasil **/
                    return Result<AssignmentSubmissionTeacherDto>.Success(request.AssignmentSubmissionTeacherDto); // Mapped automatically
                }
                catch (Exception ex)
                {
                    /** Langkah 6: Menangani kesalahan jika terjadi **/
                    return Result<AssignmentSubmissionTeacherDto>.Failure($"Failed to update AssignmentSubmission: {ex.Message}");
                }
            }
        }
    }
}