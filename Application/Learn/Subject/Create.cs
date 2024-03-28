using MediatR;
using Persistence;
using FluentValidation;
using Application.Core;
using AutoMapper;
using Domain.Learn.Subject;

namespace Application.Learn.Subject
{
    public class Create
    {
        public class Command : IRequest<Result<LessonDto>>
        {
            public LessonDto LessonDto { get; set; }
        }

        public class LessonValidator : AbstractValidator<LessonDto>
        {
            public LessonValidator()
            {
                RuleFor(x => x.LessonName).NotEmpty();
                RuleFor(x => x.UniqueNumber)
                    .NotEmpty()
                    .Matches(@"^\d{3}$") // Memastikan panjang string adalah 3 digit
                    .WithMessage("UniqueNumber must be 3 digits")
                    .Must(BeInRange)
                    .WithMessage("UniqueNumber must be in the range 001 to 100");
            }

            private bool BeInRange(string uniqueNumber)
            {
                if (int.TryParse(uniqueNumber, out int number))
                {
                    return number >= 1 && number <= 100;
                }
                return false;
            }
        }

        public class Handler : IRequestHandler<Command, Result<LessonDto>>
        {
            private readonly DataContext _context;
            private readonly IMapper _mapper;

            public Handler(DataContext context, IMapper mapper)
            {
                _context = context;
                _mapper = mapper;
            }

            public async Task<Result<LessonDto>> Handle(Command request, CancellationToken cancellationToken)
            {
                var uniqueNumber = request.LessonDto.UniqueNumber;

                // Cek apakah UniqueNumber sudah ada di database
                var isUnique = !_context.Lessons.Any(x => x.UniqueNumber == uniqueNumber);

                if (!isUnique)
                {
                    return Result<LessonDto>.Failure("UniqueNumber already exists");
                }

                var classRoom = new Lesson
                {
                    LessonName = request.LessonDto.LessonName,
                    UniqueNumber = uniqueNumber,
                };

                _context.Lessons.Add(classRoom);

                var result = await _context.SaveChangesAsync(cancellationToken) > 0;

                if (!result)
                    return Result<LessonDto>.Failure("Failed to Create Lesson");

                var classRoomDto = _mapper.Map<LessonDto>(classRoom);

                return Result<LessonDto>.Success(classRoomDto);
            }
        }
    }
}