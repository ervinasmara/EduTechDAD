﻿using MediatR;
using Application.Core;
using Application.Interface;
using FluentValidation;
using AutoMapper;
using Domain.Submission;
using Persistence;
using Microsoft.EntityFrameworkCore;

namespace Application.Submission.Command;
public class CreateSubmissionByStudentId
{
    public class Command : IRequest<Result<SubmissionCreateByStudentIdDto>>
    {
        public SubmissionCreateByStudentIdDto SubmissionDto { get; set; }
    }

    public class Handler : IRequestHandler<Command, Result<SubmissionCreateByStudentIdDto>>
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

        public async Task<Result<SubmissionCreateByStudentIdDto>> Handle(Command request, CancellationToken cancellationToken)
        {
            try
            {
                /** Langkah 1: Ambil ID Siswa dari token **/
                var studentId = Guid.Parse(_userAccessor.GetStudentIdFromToken());

                /** Langkah 2: Ambil tugas yang sesuai dari database **/
                var assignment = await _context.Assignments.FindAsync(request.SubmissionDto.AssignmentId);
                if (assignment == null)
                    return Result<SubmissionCreateByStudentIdDto>.Failure($"Tugas dengan ID {request.SubmissionDto.AssignmentId} tidak ditemukan");

                /** Langkah 3: Ambil siswa yang sesuai dari database **/
                var student = await _context.Students.FindAsync(studentId);
                if (student == null)
                    return Result<SubmissionCreateByStudentIdDto>.Failure($"Siswa dengan ID {studentId} tidak ditemukan");

                /** Langkah 4: Periksa apakah submission sudah ada sebelumnya **/
                var existingSubmission = await _context.AssignmentSubmissions
                    .FirstOrDefaultAsync(s => s.AssignmentId == assignment.Id && s.StudentId == studentId, cancellationToken);
                if (existingSubmission != null)
                    return Result<SubmissionCreateByStudentIdDto>.Failure($"Pengumpulan sudah ada untuk tugas ID {assignment.Id} and siswa ID {studentId}.");

                /** Langkah 5: Buat objek SubmissionCreateByStudentIdDto **/
                var submissionDto = new SubmissionCreateByStudentIdDto
                {
                    AssignmentId = request.SubmissionDto.AssignmentId,
                    FileData = request.SubmissionDto.FileData,
                    Link = request.SubmissionDto.Link
                };

                /** Langkah 5.1: Pemetaan dari DTO ke objek AssignmentSubmission **/
                var assignmentSubmission = _mapper.Map<AssignmentSubmission>(submissionDto);
                assignmentSubmission.StudentId = studentId;

                /** Langkah 5.2: Simpan file submission jika ada **/
                if (request.SubmissionDto.FileData != null)
                {
                    string relativeFolderPath = "Upload/FileAssignmentSubmission";
                    assignmentSubmission.FilePath = await _fileService.SaveFileSubmission(request.SubmissionDto.FileData, relativeFolderPath, DateTime.UtcNow);
                }

                /** Langkah 5.3: Validasi jenis file **/
                if (request.SubmissionDto.FileData != null)
                {
                    string fileExtension = Path.GetExtension(request.SubmissionDto.FileData.FileName);
                    if (!string.Equals(fileExtension, ".pdf", StringComparison.OrdinalIgnoreCase))
                    {
                        return Result<SubmissionCreateByStudentIdDto>.Failure("Hanya file PDF yang diperbolehkan");
                    }
                }

                /** Langkah 6: Tetapkan ID tugas dan tambahkan submission ke konteks **/
                assignmentSubmission.AssignmentId = assignment.Id;
                _context.AssignmentSubmissions.Add(assignmentSubmission);

                /** Langkah 7: Simpan perubahan ke database **/
                var result = await _context.SaveChangesAsync(cancellationToken) > 0;
                if (!result)
                    return Result<SubmissionCreateByStudentIdDto>.Failure("Gagal untuk membuat pengumpulan tugas");

                /** Langkah 8: Kembalikan hasil yang berhasil **/
                return Result<SubmissionCreateByStudentIdDto>.Success(submissionDto);
            }
            catch (Exception ex)
            {
                /** Langkah 9: Tangani kesalahan jika terjadi **/
                return Result<SubmissionCreateByStudentIdDto>.Failure($"Gagal untuk membuat pengumpulan tugas: {ex.Message}");
            }
        }
    }
}

public class CreateSubmissionByStudentIdValidator : AbstractValidator<SubmissionCreateByStudentIdDto>
{
    public CreateSubmissionByStudentIdValidator()
    {
        RuleFor(x => x.AssignmentId).NotEmpty();
        RuleFor(x => x.Link)
            .NotEmpty()
            .When(x => x.FileData == null) // Hanya memeriksa Link jika FileData kosong
            .WithMessage("AssignmentLink must be provided if FileData is not provided.");
    }
}