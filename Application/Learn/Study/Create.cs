using Application.ClassRooms;
using Application.Core;
using AutoMapper;
using Domain.Learn.Study;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Http;
using Persistence;

namespace Application.Learn.Study
{
    public class Create
    {
        public class Command : IRequest<Result<CourseDto>>
        {
            public string CourseName { get; set; }
            public string Description { get; set; }
            public IFormFile FileData { get; set; } // Properti untuk file
            public string LinkCourse { get; set; }
            public string UniqueNumber { get; set; } // Menambahkan properti UniqueNumber
        }

        public class CommandValidator : AbstractValidator<Command>
        {
            public CommandValidator()
            {
                RuleFor(x => x.CourseName).NotEmpty();
                RuleFor(x => x.Description).NotEmpty();
                RuleFor(x => x.FileData).NotEmpty();
                RuleFor(x => x.UniqueNumber)
                    .NotEmpty()
                    .Matches(@"^\d{2}$") // Memastikan panjang string adalah 3 digit
                    .WithMessage("UniqueNumber must be 2 digits")
                    .Must(BeInRange)
                    .WithMessage("UniqueNumber must be in the range 01 to 99");
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
                    // Membaca data file
                    byte[] fileData;
                    using (var memoryStream = new MemoryStream())
                    {
                        await request.FileData.CopyToAsync(memoryStream);
                        fileData = memoryStream.ToArray();
                    }

                    // Menemukan Lesson yang sesuai berdasarkan UniqueNumber
                    var lesson = _context.Lessons.FirstOrDefault(x => x.UniqueNumber == request.UniqueNumber);
                    if (lesson == null)
                        return Result<CourseDto>.Failure($"Lesson with UniqueNumber {request.UniqueNumber} not found.");

                    // Membuat objek Course dari data yang diterima
                    var course = new Course
                    {
                        CourseName = request.CourseName,
                        Description = request.Description,
                        FileData = fileData,
                        LinkCourse = request.LinkCourse,
                        LessonId = lesson.Id // Mengisi LessonId dengan Id Lesson yang sesuai
                    };

                    // Menambahkan Course ke database
                    _context.Courses.Add(course);
                    await _context.SaveChangesAsync(cancellationToken);

                    // Mengembalikan CourseDto yang baru dibuat
                    var courseDto = _mapper.Map<CourseDto>(course);
                    courseDto.UniqueNumber = lesson.UniqueNumber;
                    return Result<CourseDto>.Success(courseDto);
                }
                catch (Exception ex)
                {
                    // Menangani kesalahan dan mengembalikan pesan kesalahan yang sesuai
                    return Result<CourseDto>.Failure($"Failed to save file: {ex.Message}");
                }
            }
        }
    }
}