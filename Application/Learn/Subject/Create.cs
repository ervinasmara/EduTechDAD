using MediatR;
using Persistence;
using FluentValidation;
using Application.Core;
using AutoMapper;
using Domain.Learn.Subject;
using Microsoft.EntityFrameworkCore;

namespace Application.Learn.Subject
{
    public class Create
    {
        public class Command : IRequest<Result<LessonCreateDto>>
        {
            public LessonCreateDto LessonCreateDto { get; set; }
        }

        public class CommandLesson : AbstractValidator<Command>
        {
            public CommandLesson()
            {
                RuleFor(x => x.LessonCreateDto).SetValidator(new LessonCreateValidator());
            }
        }

        public class Handler : IRequestHandler<Command, Result<LessonCreateDto>>
        {
            private readonly DataContext _context;
            private readonly IMapper _mapper;

            public Handler(DataContext context, IMapper mapper)
            {
                _context = context;
                _mapper = mapper;
            }

            public async Task<Result<LessonCreateDto>> Handle(Command request, CancellationToken cancellationToken)
            {
                var lastLesson = await _context.Lessons
                                                .OrderByDescending(x => x.UniqueNumberOfLesson)
                                                .FirstOrDefaultAsync(cancellationToken);

                int newUniqueNumber = 1; // Nilai awal jika tidak ada lesson sebelumnya

                if (lastLesson != null)
                {
                    // Ambil nomor terakhir dan tambahkan 1
                    var lastUniqueNumber = int.Parse(lastLesson.UniqueNumberOfLesson);
                    newUniqueNumber = lastUniqueNumber + 1;
                }

                // Buat UniqueNumberOfLesson dengan format 2 digit
                var uniqueNumber = newUniqueNumber.ToString("00");

                var classRoom = new Lesson
                {
                    LessonName = request.LessonCreateDto.LessonName,
                    UniqueNumberOfLesson = uniqueNumber,
                };

                _context.Lessons.Add(classRoom);

                var result = await _context.SaveChangesAsync(cancellationToken) > 0;

                if (!result)
                    return Result<LessonCreateDto>.Failure("Failed to Create Lesson");

                var classRoomDto = _mapper.Map<LessonCreateDto>(classRoom);

                return Result<LessonCreateDto>.Success(classRoomDto);
            }

        }
    }
}