using Application.Core;
using AutoMapper;
using Domain.Learn.Schedules;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Learn.Schedules
{
    public class Create
    {
        public class Command : IRequest<Result<ScheduleDto>>
        {
            public ScheduleDto ScheduleDto { get; set; }
        }

        public class CommandValidator : AbstractValidator<Command>
        {
            public CommandValidator()
            {
                RuleFor(x => x.ScheduleDto).SetValidator(new ScheduleValidator());
            }
        }

        public class Handler : IRequestHandler<Command, Result<ScheduleDto>>
        {
            private readonly DataContext _context;
            private readonly IMapper _mapper;

            public Handler(DataContext context, IMapper mapper)
            {
                _context = context;
                _mapper = mapper;
            }

            public async Task<Result<ScheduleDto>> Handle(Command request, CancellationToken cancellationToken)
            {
                // Cari Lesson berdasarkan UniqueNumberOfLesson
                var lesson = await _context.Lessons.SingleOrDefaultAsync(l => l.UniqueNumberOfLesson == request.ScheduleDto.UniqueNumberOfLesson);
                if (lesson == null)
                    return Result<ScheduleDto>.Failure("Lesson not found with the provided UniqueNumberOfLesson");

                // Cari ClassRoom berdasarkan UniqueNumberOfClassRoom
                var classRoom = await _context.ClassRooms.SingleOrDefaultAsync(cr => cr.UniqueNumberOfClassRoom == request.ScheduleDto.UniqueNumberOfClassRoom);
                if (classRoom == null)
                    return Result<ScheduleDto>.Failure("ClassRoom not found with the provided UniqueNumberOfClassRoom");

                var schedule = new Schedule
                {
                    Day = request.ScheduleDto.Day,
                    StartTime = request.ScheduleDto.StartTime,
                    EndTime = request.ScheduleDto.EndTime,
                    LessonId = lesson.Id, // Setel LessonId dengan Id dari Lesson yang ditemukan
                    ClassRoomId = classRoom.Id // Setel ClassRoomId dengan Id dari ClassRoom yang ditemukan
                };

                _context.Schedules.Add(schedule);

                var result = await _context.SaveChangesAsync(cancellationToken) > 0;

                if (!result) return Result<ScheduleDto>.Failure("Failed to create Schedule");

                // Buat DTO dari jadwal yang telah dibuat
                var scheduleDto = _mapper.Map<ScheduleDto>(schedule);

                // Tambahkan UniqueNumberOfLesson dan UniqueNumberOfClassRoom ke dalam response body
                scheduleDto.UniqueNumberOfLesson = lesson.UniqueNumberOfLesson;
                scheduleDto.UniqueNumberOfClassRoom = classRoom.UniqueNumberOfClassRoom;

                return Result<ScheduleDto>.Success(scheduleDto);
            }
        }
    }
}