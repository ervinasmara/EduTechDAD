using Application.Core;
using Application.Interface;
using AutoMapper;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Assignments.Command;
public class EditAssignment
{
    public class Command : IRequest<Result<AssignmentCreateAndEditDto>>
    {
        public Guid AssignmentId { get; set; }
        public AssignmentCreateAndEditDto AssignmentCreateAndEditDto { get; set; }
    }

    public class CommandValidatorDto : AbstractValidator<Command>
    {
        public CommandValidatorDto()
        {
            RuleFor(x => x.AssignmentCreateAndEditDto).SetValidator(new AssignmentValidator());
        }
    }

    public class Handler : IRequestHandler<Command, Result<AssignmentCreateAndEditDto>>
    {
        private readonly DataContext _context;
        private readonly IUserAccessor _userAccessor;
        private readonly IMapper _mapper;
        private readonly IFileService _fileService;

        public Handler(DataContext context, IUserAccessor userAccessor, IMapper mapper, IFileService fileService)
        {
            _context = context;
            _userAccessor = userAccessor;
            _mapper = mapper;
            _fileService = fileService;
        }

        public async Task<Result<AssignmentCreateAndEditDto>> Handle(Command request, CancellationToken cancellationToken)
        {
            /** Step 1: Get TeacherId dari token **/
            var teacherId = _userAccessor.GetTeacherIdFromToken();

            if (teacherId == null)
            {
                return Result<AssignmentCreateAndEditDto>.Failure("TeacherId tidak ditemukan ditoken");
            }

            /** Step 2: Cek apakah Teacher punya akses ke Course yang dipilih **/
            var courseId = request.AssignmentCreateAndEditDto.CourseId;
            var isTeacherHasAccessToCourse = await _context.TeacherLessons
                .AnyAsync(tl => tl.TeacherId == Guid.Parse(teacherId) && tl.Lesson.Courses.Any(c => c.Id == courseId));

            if (!isTeacherHasAccessToCourse)
            {
                return Result<AssignmentCreateAndEditDto>.Failure("Teacher does not have access to this Course");
            }

            /** Step 3: Temukan Assignment by ID **/
            var assignment = await _context.Assignments.FindAsync(request.AssignmentId);

            if (assignment == null)
            {
                return Result<AssignmentCreateAndEditDto>.Failure("Tugas tidak ditemukan");
            }

            /** Langkah 4 (Opsional): Menangani pengunggahan file **/
            string assignmentFilePath = null;
            if (request.AssignmentCreateAndEditDto.AssignmentFileData != null)
            {
                string fileExtension = Path.GetExtension(request.AssignmentCreateAndEditDto.AssignmentFileData.FileName);
                if (!string.Equals(fileExtension, ".pdf", StringComparison.OrdinalIgnoreCase))
                {
                    return Result<AssignmentCreateAndEditDto>.Failure("Hanya file PDF yang diperbolehkan");
                }

                string relativeFolderPath = "Upload/FileAssignment";
                assignmentFilePath = await _fileService.SaveFileAsync(request.AssignmentCreateAndEditDto.AssignmentFileData,
                    relativeFolderPath, request.AssignmentCreateAndEditDto.AssignmentName, assignment.CreatedAt);
            }

            /** Langkah 5: Perbarui properti Assignment menggunakan AutoMapper **/
            _mapper.Map(request.AssignmentCreateAndEditDto, assignment);

            // **Note:** Baris ini memperbarui properti `assignment` yang ada dengan nilai dari DTO.

            // Tetapkan FilePath dengan nilai yang disimpan (jika ada)
            assignment.FilePath = assignmentFilePath;

            /** Langkah 6: Menyimpan perubahan ke basis data **/
            var result = await _context.SaveChangesAsync(cancellationToken) > 0;
            if (!result)
            {
                return Result<AssignmentCreateAndEditDto>.Failure("Gagal menyimpan perubahan tugas.");
            }

            if (!result)
            {
                return Result<AssignmentCreateAndEditDto>.Failure("Gagal untuk mengupdate tugas");
            }

            /** Langkah 7: Kembalikan respons sukses dengan DTO **/
            return Result<AssignmentCreateAndEditDto>.Success(request.AssignmentCreateAndEditDto);
        }
    }
}