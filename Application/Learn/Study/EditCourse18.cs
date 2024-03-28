using Application.ClassRooms;
using Application.Core;
using AutoMapper;
using Domain.Class;
using Domain.Learn.Study;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Http;
using Persistence;

namespace Application.Learn.Study
{
    public class EditCourse18
    {
        public class Command : IRequest<Result<CourseDto>>
        {
            public Guid Id { get; set; }
            public string CourseName { get; set; }
            public string Description { get; set; }
            public IFormFile FileData { get; set; } // Properti untuk file
            public string LinkCourse { get; set; }
            public string UniqueNumber { get; set; }
        }


        public class CommandValidator : AbstractValidator<Command>
        {
            public CommandValidator()
            {
                RuleFor(x => x.CourseName).NotEmpty();
                RuleFor(x => x.Description).NotEmpty();
                RuleFor(x => x.UniqueNumber)
                    .NotEmpty()
                    .Matches(@"^\d{2}$") // Memastikan panjang string adalah 2 digit
                    .WithMessage("UniqueNumber must be 2 digits")
                    .Must(BeInRange)
                    .WithMessage("UniqueNumber must be in the range 01 to 99");

                // Validasi untuk memastikan bahwa setidaknya satu dari FileData atau LinkCourse diisi
                RuleFor(x => x).Custom((command, context) =>
                {
                    if (command.FileData == null && string.IsNullOrEmpty(command.LinkCourse))
                    {
                        context.AddFailure("Either FileData or LinkCourse must be provided.");
                    }
                });
            }

            private bool BeInRange(string uniqueNumber)
            {
                if (int.TryParse(uniqueNumber, out int number))
                {
                    return number >= 1 && number <= 99;
                }
                return false;
            }
        }

        public class Handler : IRequestHandler<Command, Result<CourseDto>>
        {
            private readonly DataContext _context;
            private readonly IMapper _mapper;

            public Handler(DataContext context, IMapper mapper)
            {
                _context = context;
                _mapper = mapper;
            }

            public async Task<Result<CourseDto>> Handle(Command request, CancellationToken cancellationToken)
            {
                try
                {
                    // Membaca data file jika FileData tidak null
                    byte[] fileData = null;
                    if (request.FileData != null)
                    {
                        using (var memoryStream = new MemoryStream())
                        {
                            await request.FileData.CopyToAsync(memoryStream);
                            fileData = memoryStream.ToArray();
                        }
                    }

                    // Menemukan Course yang akan diubah berdasarkan ID
                    var course = await _context.Courses.FindAsync(request.Id);
                    if (course == null)
                        return Result<CourseDto>.Failure($"Course with ID {request.Id} not found.");

                    // Mengisi properti yang akan diubah dengan nilai yang diberikan
                    course.CourseName = request.CourseName;
                    course.Description = request.Description;
                    course.FileData = fileData ?? course.FileData; // Memperbarui file jika FileData tidak null
                    course.LinkCourse = request.LinkCourse;

                    // Jika UniqueNumber juga diubah, Anda perlu menangani perubahan ini sesuai dengan kebutuhan Anda
                    // Misalnya, Anda dapat mencari lesson yang sesuai dengan UniqueNumber baru dan mengganti LessonId
                    // Atau jika UniqueNumber tidak berubah, Anda bisa melewatkannya

                    // Menyimpan perubahan ke database
                    await _context.SaveChangesAsync(cancellationToken);

                    // Mengembalikan CourseDto yang telah diubah
                    var courseDto = _mapper.Map<CourseDto>(course);
                    return Result<CourseDto>.Success(courseDto);
                }
                catch (Exception ex)
                {
                    // Menangani kesalahan dan mengembalikan pesan kesalahan yang sesuai
                    return Result<CourseDto>.Failure($"Failed to update course: {ex.Message}");
                }
            }
        }
    }
}