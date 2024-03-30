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

        public class CommandLesson : AbstractValidator<Command>
        {
            public CommandLesson()
            {
                RuleFor(x => x.LessonDto).SetValidator(new LessonValidator());
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
                var uniqueNumber = request.LessonDto.UniqueNumberOfLesson;

                // Cek apakah UniqueNumberOfLesson sudah ada di database
                var isUnique = !_context.Lessons.Any(x => x.UniqueNumberOfLesson == uniqueNumber);

                if (!isUnique)
                {
                    return Result<LessonDto>.Failure("UniqueNumberOfLesson already exists");
                }

                var classRoom = new Lesson
                {
                    LessonName = request.LessonDto.LessonName,
                    UniqueNumberOfLesson = uniqueNumber,
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