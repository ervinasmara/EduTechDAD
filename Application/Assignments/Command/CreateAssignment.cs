using Application.Core;
using Application.Interface;
using AutoMapper;
using Domain.Assignments;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Assignments.Command;
public class CreateAssignment
{
    public class Command : IRequest<Result<AssignmentCreateDto>>
    {
        public AssignmentCreateDto AssignmentCreateDto { get; set; }
    }

    public class Handler : IRequestHandler<Command, Result<AssignmentCreateDto>>
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

        public async Task<Result<AssignmentCreateDto>> Handle(Command request, CancellationToken cancellationToken)
        {
            /** Langkah 1: Dapatkan TeacherId dari token **/
            var teacherId = _userAccessor.GetTeacherIdFromToken();

            if (teacherId == null)
            {
                return Result<AssignmentCreateDto>.Failure("TeacherId tidak ditemukan ditoken");
            }

            /** Langkah 2: Periksa akses Guru ke Course **/
            var courseId = request.AssignmentCreateDto.CourseId;
            var isTeacherHasAccessToCourse = await _context.TeacherLessons
                .AnyAsync(tl => tl.TeacherId == Guid.Parse(teacherId) && tl.Lesson.Courses.Any(c => c.Id == courseId));

            if (!isTeacherHasAccessToCourse)
            {
                return Result<AssignmentCreateDto>.Failure("Guru tidak memiliki akses ke Materi ini");
            }

            /** Langkah 3: Buat objek Assignment dari DTO menggunakan AutoMapper **/
            var assignment = _mapper.Map<Assignment>(request.AssignmentCreateDto);

            /** Langkah 4 (Opsional): Simpan file dan dapatkan path **/
            string filePath = null;
            if (request.AssignmentCreateDto.AssignmentFileData != null)
            {
                string fileExtension = Path.GetExtension(request.AssignmentCreateDto.AssignmentFileData.FileName);
                if (!string.Equals(fileExtension, ".pdf", StringComparison.OrdinalIgnoreCase))
                {
                    return Result<AssignmentCreateDto>.Failure("Hanya file PDF yang diperbolehkan");
                }

                string relativeFolderPath = "/app/uploads/assignment";
                filePath = await _fileService.SaveFileAsync(request.AssignmentCreateDto.AssignmentFileData,
                    relativeFolderPath, request.AssignmentCreateDto.AssignmentName, assignment.CreatedAt);

                // Tetapkan nilai filePath ke properti FilePath dari objek assignment
                assignment.FilePath = filePath;
            }

            /** Langkah 5: Tambahkan Assignment ke Course **/
            var course = await _context.Courses.FindAsync(courseId);
            if (course == null)
            {
                return Result<AssignmentCreateDto>.Failure("Materi tidak ditemukan");
            }

            course.Assignments ??= new List<Assignment>(); // Pastikan koleksi Assignments tidak null

            course.Assignments.Add(assignment);

            /** Langkah 6: Simpan perubahan ke database **/
            var result = await _context.SaveChangesAsync(cancellationToken) > 0;

            if (!result)
            {
                return Result<AssignmentCreateDto>.Failure("Gagal untuk membuat tugas");
            }

            /** Langkah 7: Kirim kembali DTO sebagai respons **/
            return Result<AssignmentCreateDto>.Success(request.AssignmentCreateDto);
        }
    }
}

public class CommandValidatorCreateDto : AbstractValidator<AssignmentCreateDto>
{
    public CommandValidatorCreateDto()
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

        RuleFor(x => x.AssignmentLink)
            .NotEmpty()
            .When(x => x.AssignmentFileData == null) // Hanya memeriksa AssignmentLink jika FileData kosong
            .WithMessage("Link Tugas harus disediakan jika FileData tidak disediakan.");
    }
}