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
    public class Edit
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
        _mapper = mapper;
        _context = context;
    }

    public async Task<Result<CourseDto>> Handle(Command request, CancellationToken cancellationToken)
    {
        var course = await _context.Courses.FindAsync(request.Id);

        // Periksa apakah course ditemukan
        if (course == null)
        {
            return Result<CourseDto>.Failure("Course Not Found");
        }

        // Memetakan data dari Command ke entitas Course
        _mapper.Map(request, course);

        // Tidak perlu konversi data file

        var result = await _context.SaveChangesAsync(cancellationToken) > 0;

        if (!result)
        {
            return Result<CourseDto>.Failure("Gagal untuk mengedit Course");
        }

        // Buat instance CourseDto yang mewakili hasil edit
        var editedCourseDto = _mapper.Map<CourseDto>(course);

        return Result<CourseDto>.Success(editedCourseDto);
    }
}
    }
}