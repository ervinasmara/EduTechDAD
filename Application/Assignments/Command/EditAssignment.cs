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
    public class Command : IRequest<Result<AssignmentEditDto>>
    {
        public Guid AssignmentId { get; set; }
        public AssignmentEditDto AssignmentEditDto { get; set; }
    }

    public class Handler : IRequestHandler<Command, Result<AssignmentEditDto>>
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

        public async Task<Result<AssignmentEditDto>> Handle(Command request, CancellationToken cancellationToken)
        {
            /** Step 1: Get TeacherId dari token **/
            var teacherId = _userAccessor.GetTeacherIdFromToken();

            if (teacherId == null)
            {
                return Result<AssignmentEditDto>.Failure("TeacherId tidak ditemukan ditoken");
            }

            /** Step 2: Cek apakah Teacher punya akses ke Course yang dipilih **/
            var courseId = request.AssignmentEditDto.CourseId;
            var isTeacherHasAccessToCourse = await _context.TeacherLessons
                .AnyAsync(tl => tl.TeacherId == Guid.Parse(teacherId) && tl.Lesson.Courses.Any(c => c.Id == courseId));

            if (!isTeacherHasAccessToCourse)
            {
                return Result<AssignmentEditDto>.Failure("Teacher does not have access to this Course");
            }

            /** Step 3: Temukan Assignment by ID **/
            var assignment = await _context.Assignments.FindAsync(request.AssignmentId);

            if (assignment == null)
            {
                return Result<AssignmentEditDto>.Failure("Tugas tidak ditemukan");
            }

            /** Langkah 4 (Opsional): Menangani pengunggahan file **/
            //string assignmentFilePath = null;
            //if (request.AssignmentEditDto.AssignmentFileData != null)
            //{
            //    string fileExtension = Path.GetExtension(request.AssignmentEditDto.AssignmentFileData.FileName);
            //    if (!string.Equals(fileExtension, ".pdf", StringComparison.OrdinalIgnoreCase))
            //    {
            //        return Result<AssignmentEditDto>.Failure("Hanya file PDF yang diperbolehkan");
            //    }

            //    string relativeFolderPath = "Upload/FileAssignment";
            //    assignmentFilePath = await _fileService.SaveFileAsync(request.AssignmentEditDto.AssignmentFileData,
            //        relativeFolderPath, request.AssignmentEditDto.AssignmentName, assignment.CreatedAt);
            //}

            /** Langkah 5: Perbarui properti Assignment menggunakan AutoMapper **/
            _mapper.Map(request.AssignmentEditDto, assignment);

            // Tetapkan FilePath dengan nilai yang disimpan (jika ada)
            //assignment.FilePath = assignmentFilePath;

            /** Langkah 6: Menyimpan perubahan ke basis data **/
            var result = await _context.SaveChangesAsync(cancellationToken) > 0;
            if (!result)
            {
                return Result<AssignmentEditDto>.Failure("Gagal menyimpan perubahan tugas.");
            }

            if (!result)
            {
                return Result<AssignmentEditDto>.Failure("Gagal untuk mengupdate tugas");
            }

            /** Langkah 7: Kembalikan respons sukses dengan DTO **/
            return Result<AssignmentEditDto>.Success(request.AssignmentEditDto);
        }
    }
}

public class CommandValidatorDto : AbstractValidator<AssignmentEditDto>
{
    public CommandValidatorDto()
    {
        RuleFor(x => x.AssignmentName).NotEmpty().WithMessage("Nama tugas tidak boleh kosong");
        RuleFor(x => x.AssignmentDate).NotEmpty().WithMessage("Tanggal tugas tidak boleh kosong");
        RuleFor(x => x.AssignmentDeadline).NotEmpty().WithMessage("Tenggat waktu tugas tidak boleh kosong");
        RuleFor(x => x.AssignmentDescription).NotEmpty().WithMessage("Deskripsi tugas tidak boleh kosong");
        RuleFor(x => x.CourseId).NotEmpty().WithMessage("Materi tugas tidak boleh kosong");
        RuleFor(x => x.TypeOfSubmission)
            .NotEmpty().WithMessage("Tipe pengumpulan tidak boleh kosong")
            .Must(x => x == 1 || x == 2)
            .WithMessage("Jenis pengiriman harus berupa 1 (untuk file) atau 2 (untuk tautan).");
    }
}