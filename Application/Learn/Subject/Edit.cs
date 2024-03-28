using MediatR;
using Persistence;
using AutoMapper;
using FluentValidation;
using Application.Core;
using Domain.Learn.Subject;

namespace Application.Learn.Subject
{
    public class Edit
    {
        public class Command : IRequest<Result<LessonDto>>
        {
            public Guid Id { get; set; }
            public LessonDto LessonDto { get; set; }
        }

        public class CommandValidatorDto : AbstractValidator<Command>
        {
            public CommandValidatorDto()
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
                _mapper = mapper;
                _context = context;
            }

            public async Task<Result<LessonDto>> Handle(Command request, CancellationToken cancellationToken)
            {
                var lesson = await _context.Lessons.FindAsync(request.Id);

                // Periksa apakah lesson ditemukan
                if (lesson == null)
                {
                    return Result<LessonDto>.Failure("Lesson Not Found");
                }

                _mapper.Map(request.LessonDto, lesson);

                var result = await _context.SaveChangesAsync(cancellationToken) > 0;

                if (!result)
                {
                    return Result<LessonDto>.Failure("Failed to edit Lesson");
                }

                // Buat instance LessonDto yang mewakili hasil edit
                var editedLessonDto = _mapper.Map<LessonDto>(lesson);

                return Result<LessonDto>.Success(editedLessonDto);
            }
        }
    }
}
