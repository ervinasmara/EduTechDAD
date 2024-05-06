using Application.Core;
using AutoMapper;
using Domain.Learn.Schedules;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Learn.Schedules.Command;
public class CreateSchedule
{
    public class Command : IRequest<Result<ScheduleCreateAndEditDto>>
    {
        public ScheduleCreateAndEditDto ScheduleCreateAndEditDto { get; set; }
    }

    public class CommandValidator : AbstractValidator<Command>
    {
        public CommandValidator()
        {
            RuleFor(x => x.ScheduleCreateAndEditDto).SetValidator(new ScheduleCreateAndEditValidator());
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
                /** Langkah 1: Temukan Pelajaran Berdasarkan Nama Pelajaran yang Diberikan **/
                var lesson = await _context.Lessons
                    .FirstOrDefaultAsync(l => l.LessonName == request.ScheduleCreateAndEditDto.LessonName, cancellationToken);

                /** Langkah 2: Memeriksa Ketersediaan Pelajaran **/
                if (lesson == null)
                    return Result<ScheduleCreateAndEditDto>.Failure($"Lesson with name '{request.ScheduleCreateAndEditDto.LessonName}' not found.");

                /** Langkah 3: Membuat Instance Jadwal dari ScheduleCreateAndEditDto dan Mengatur LessonId **/
                var schedule = _mapper.Map<Schedule>(request.ScheduleCreateAndEditDto);
                schedule.LessonId = lesson.Id;

                /** Langkah 4: Menambahkan Jadwal ke Database **/
                _context.Schedules.Add(schedule);

                /** Langkah 5: Menyimpan Perubahan ke Database **/
                var result = await _context.SaveChangesAsync(cancellationToken) > 0;

                /** Langkah 6: Memeriksa Hasil Simpan **/
                if (!result)
                    return Result<ScheduleCreateAndEditDto>.Failure("Failed to create Schedule");

                /** Langkah 7: Mengembalikan Hasil dalam Bentuk Success Result **/
                var scheduleDto = _mapper.Map<ScheduleCreateAndEditDto>(schedule);

                return Result<ScheduleCreateAndEditDto>.Success(scheduleDto);
            }
            catch (Exception ex)
            {
                /** Langkah 8: Menangani Kesalahan Jika Terjadi **/
                return Result<ScheduleCreateAndEditDto>.Failure($"Failed to create schedule: {ex.Message}");
            }
        }
    }
}