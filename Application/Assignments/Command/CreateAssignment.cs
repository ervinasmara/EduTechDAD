using Application.Core;
using Application.Interface;
using AutoMapper;
using Domain.Assignments;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Assignments.Command
{
    public class CreateAssignment
    {
        public class Command : IRequest<Result<AssignmentCreateAndEditDto>>
        {
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
                /** Langkah 1: Dapatkan TeacherId dari token **/
                var teacherId = _userAccessor.GetTeacherIdFromToken();

                if (teacherId == null)
                {
                    return Result<AssignmentCreateAndEditDto>.Failure("Teacher ID not found in token");
                }

                /** Langkah 2: Periksa akses Guru ke Course **/
                var courseId = request.AssignmentCreateAndEditDto.CourseId;
                var isTeacherHasAccessToCourse = await _context.TeacherLessons
                    .AnyAsync(tl => tl.TeacherId == Guid.Parse(teacherId) && tl.Lesson.Courses.Any(c => c.Id == courseId));

                if (!isTeacherHasAccessToCourse)
                {
                    return Result<AssignmentCreateAndEditDto>.Failure("Teacher does not have access to this Course");
                }

                /** Langkah 3: Buat objek Assignment dari DTO menggunakan AutoMapper **/
                var assignment = _mapper.Map<Assignment>(request.AssignmentCreateAndEditDto);

                /** Langkah 4 (Opsional): Simpan file dan dapatkan path **/
                if (request.AssignmentCreateAndEditDto.AssignmentFileData != null)
                {
                    string relativeFolderPath = "Upload/FileAssignment";
                    assignment.FilePath = await _fileService.SaveFileAsync(request.AssignmentCreateAndEditDto.AssignmentFileData,
                        relativeFolderPath, request.AssignmentCreateAndEditDto.AssignmentName, assignment.CreatedAt);
                }

                if (request.AssignmentCreateAndEditDto.AssignmentFileData != null)
                {
                    string fileExtension = Path.GetExtension(request.AssignmentCreateAndEditDto.AssignmentFileData.FileName);
                    if (!string.Equals(fileExtension, ".pdf", StringComparison.OrdinalIgnoreCase))
                    {
                        return Result<AssignmentCreateAndEditDto>.Failure("Only PDF files are allowed.");
                    }
                }

                /** Langkah 5: Tambahkan Assignment ke Course **/
                var course = await _context.Courses.FindAsync(courseId);
                if (course == null)
                {
                    return Result<AssignmentCreateAndEditDto>.Failure("Course not found");
                }

                course.Assignments ??= new List<Assignment>(); // Pastikan koleksi Assignments tidak null

                course.Assignments.Add(assignment);

                /** Langkah 6: Simpan perubahan ke database **/
                var result = await _context.SaveChangesAsync(cancellationToken) > 0;

                if (!result)
                {
                    return Result<AssignmentCreateAndEditDto>.Failure("Failed to create Assignment");
                }

                /** Langkah 7: Kirim kembali DTO sebagai respons **/
                return Result<AssignmentCreateAndEditDto>.Success(request.AssignmentCreateAndEditDto);
            }
        }
    }
}