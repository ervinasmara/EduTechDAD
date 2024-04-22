using Application.Core;
using AutoMapper;
using FluentValidation;
using MediatR;
using Persistence;

namespace Application.Submission
{
    public class UpdateStudent1
    {
        public class Command : IRequest<Result<Unit>>
        {
            public Guid Id { get; set; } // ID AssignmentSubmission
            public AssignmentSubmissionStudentDto AssignmentSubmissionStudentDto { get; set; }
        }

        public class CommandValidator : AbstractValidator<Command>
        {
            public CommandValidator()
            {
                RuleFor(x => x.AssignmentSubmissionStudentDto).SetValidator(new AssignmentSubmissionStudentValidator());
            }
        }

        public class Handler : IRequestHandler<Command, Result<Unit>>
        {
            private readonly DataContext _context;
            private readonly IMapper _mapper;

            public Handler(DataContext context, IMapper mapper)
            {
                _context = context;
                _mapper = mapper;
            }

            public async Task<Result<Unit>> Handle(Command request, CancellationToken cancellationToken)
            {
                try
                {
                    // Cari AssignmentSubmission berdasarkan ID
                    var assignmentSubmission = await _context.AssignmentSubmissions.FindAsync(request.Id);

                    if (assignmentSubmission == null)
                        return Result<Unit>.Failure($"AssignmentSubmission with ID {request.Id} not found");

                    // Menggunakan DateTime.UtcNow dan menambahkan 7 jam (7 * 60 menit)
                    var submissionTime = DateTime.UtcNow.AddHours(7);

                    // Kemudian bisa menyimpan submissionTime ke database
                    assignmentSubmission.SubmissionTime = submissionTime;

                    // Mengupdate FileData jika disediakan
                    if (request.AssignmentSubmissionStudentDto.FileData != null)
                    {
                        using (var memoryStream = new MemoryStream())
                        {
                            await request.AssignmentSubmissionStudentDto.FileData.CopyToAsync(memoryStream);
                            assignmentSubmission.FileData = memoryStream.ToArray();
                        }
                    }

                    // Mengupdate Link jika disediakan
                    assignmentSubmission.Link = request.AssignmentSubmissionStudentDto.Link;

                    await _context.SaveChangesAsync(cancellationToken);

                    return Result<Unit>.Success(Unit.Value);
                }
                catch (Exception ex)
                {
                    // Menangani pengecualian dan mengembalikan pesan kesalahan yang sesuai
                    return Result<Unit>.Failure($"Failed to update AssignmentSubmission: {ex.Message}");
                }
            }
        }
    }
}