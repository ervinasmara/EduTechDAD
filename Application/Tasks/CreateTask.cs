using Application.Core;
using AutoMapper;
using Domain.Task;
using FluentValidation;
using MediatR;
using Persistence;

namespace Application.Tasks
{
    public class CreateTask
    {
        public class Command : IRequest<Result<AssignmentDto>>
        {
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
                    // Periksa apakah Course dengan CourseId yang diberikan ada
                    var course = await _context.Courses.FindAsync(request.AssignmentDto.CourseId);
                    if (course == null)
                        return Result<AssignmentDto>.Failure($"Course with ID {request.AssignmentDto.CourseId} not found.");

                    // Inisialisasi fileData dengan null
                    byte[]? fileData = null;

                    // Baca data file jika disediakan
                    if (request.AssignmentDto.AssignmentFileData != null)
                    {
                        using (var memoryStream = new MemoryStream())
                        {
                            await request.AssignmentDto.AssignmentFileData.CopyToAsync(memoryStream);
                            fileData = memoryStream.ToArray();
                        }
                    }

                    // Buat entitas Penugasan
                    var assignment = new Assignment
                    {
                        AssignmentName = request.AssignmentDto.AssignmentName,
                        AssignmentDate = request.AssignmentDto.AssignmentDate,
                        AssignmentDeadline = request.AssignmentDto.AssignmentDeadline,
                        AssignmentDescription = request.AssignmentDto.AssignmentDescription,
                        FileData = fileData,
                        AssignmentLink = request.AssignmentDto.AssignmentLink,
                        CourseId = course.Id
                    };

                    // Tambah Assignment ke database
                    _context.Assignments.Add(assignment);
                    await _context.SaveChangesAsync(cancellationToken);

                    // Memetakan entitas Assignment ke AssignmentDto
                    var assignmentDto = _mapper.Map<AssignmentDto>(assignment);
                    assignmentDto.CourseId = course.Id;
                    return Result<AssignmentDto>.Success(assignmentDto);
                }
                catch (Exception ex)
                {
                    // Menangani pengecualian dan mengembalikan pesan kesalahan yang sesuai
                    return Result<AssignmentDto>.Failure($"Failed to create assignment: {ex.Message}");
                }
            }
        }
    }
}