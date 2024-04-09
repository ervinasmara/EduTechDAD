using Application.Core;
using AutoMapper;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Submission
{
    public class UpdateStudent2
    {
        public class Command : IRequest<Result<AssignmentSubmissionStudentDto>>
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

        public class Handler : IRequestHandler<Command, Result<AssignmentSubmissionStudentDto>>
        {
            private readonly DataContext _context;
            private readonly IMapper _mapper;

            public Handler(DataContext context, IMapper mapper)
            {
                _context = context;
                _mapper = mapper;
            }

            public async Task<Result<AssignmentSubmissionStudentDto>> Handle(Command request, CancellationToken cancellationToken)
            {
                try
                {
                    // Cari AssignmentSubmission berdasarkan ID
                    var assignmentSubmission = await _context.AssignmentSubmissions.FindAsync(request.Id);

                    if (assignmentSubmission == null)
                        return Result<AssignmentSubmissionStudentDto>.Failure($"AssignmentSubmission with ID {request.Id} not found");

                    // Menggunakan DateTime.UtcNow dan menambahkan 7 jam (7 * 60 menit)
                    var submissionTime = DateTime.UtcNow.AddHours(7);

                    // Kemudian bisa menyimpan submissionTime ke database
                    assignmentSubmission.SubmissionTime = submissionTime;

                    assignmentSubmission.Status = 2; // Mengubah status menjadi 2 (sudah dikerjakan)

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

                    // Menandai entitas sebagai dimodifikasi
                    _context.Entry(assignmentSubmission).State = EntityState.Modified;

                    await _context.SaveChangesAsync(cancellationToken);

                    // Memetakan entitas AssignmentSubmission ke AssignmentSubmissionStudentDto
                    var updatedDto = _mapper.Map<AssignmentSubmissionStudentDto>(assignmentSubmission);

                    return Result<AssignmentSubmissionStudentDto>.Success(updatedDto);
                }
                catch (Exception ex)
                {
                    // Menangani pengecualian dan mengembalikan pesan kesalahan yang sesuai
                    return Result<AssignmentSubmissionStudentDto>.Failure($"Failed to update AssignmentSubmission: {ex.Message}");
                }
            }
        }
    }
}
