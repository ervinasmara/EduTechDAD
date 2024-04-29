using Application.Core;
using AutoMapper;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Learn.Schedules.Command
{
    public class EditSchedule
    {
        public class Command : IRequest<Result<ScheduleCreateAndEditDto>>
        {
            public Guid ScheduleId { get; set; }
            public ScheduleCreateAndEditDto ScheduleCreateAndEditDto { get; set; }
        }

        public class CommandValidator : AbstractValidator<Command>
        {
            public CommandValidator()
            {
                RuleFor(x => x.ScheduleId).NotEmpty();
                RuleFor(x => x.ScheduleCreateAndEditDto).SetValidator(new ScheduleValidator());
            }
        }

        public class Handler : IRequestHandler<Command, Result<ScheduleCreateAndEditDto>>
        {
            private readonly DataContext _context;
            private readonly IMapper _mapper;

            public Handler(DataContext context, IMapper mapper)
            {
                _context = context;
                _mapper = mapper;
            }

            public async Task<Result<ScheduleCreateAndEditDto>> Handle(Command request, CancellationToken cancellationToken)
            {
                try
                {
                    // Temukan jadwal yang akan diedit
                    var schedule = await _context.Schedules.FindAsync(request.ScheduleId);

                    if (schedule == null)
                        return Result<ScheduleCreateAndEditDto>.Failure($"Schedule with id '{request.ScheduleId}' not found.");

                    // Temukan lesson berdasarkan LessonName yang diberikan
                    var lesson = await _context.Lessons
                        .FirstOrDefaultAsync(l => l.LessonName == request.ScheduleCreateAndEditDto.LessonName, cancellationToken);

                    if (lesson == null)
                        return Result<ScheduleCreateAndEditDto>.Failure($"Lesson with name '{request.ScheduleCreateAndEditDto.LessonName}' not found.");

                    // Perbarui properti jadwal
                    schedule.Day = request.ScheduleCreateAndEditDto.Day;
                    schedule.StartTime = request.ScheduleCreateAndEditDto.StartTime;
                    schedule.EndTime = request.ScheduleCreateAndEditDto.EndTime;
                    schedule.LessonId = lesson.Id;

                    // Simpan perubahan ke database
                    var result = await _context.SaveChangesAsync(cancellationToken) > 0;

                    if (!result)
                        return Result<ScheduleCreateAndEditDto>.Failure("Failed to edit Schedule");

                    // Buat DTO respons dan kembalikan
                    var scheduleDto = _mapper.Map<ScheduleCreateAndEditDto>(schedule);
                    scheduleDto.LessonName = lesson.LessonName; // Set LessonName in response

                    return Result<ScheduleCreateAndEditDto>.Success(scheduleDto);
                }
                catch (Exception ex)
                {
                    return Result<ScheduleCreateAndEditDto>.Failure($"Failed to edit schedule: {ex.Message}");
                }
            }
        }
    }
}