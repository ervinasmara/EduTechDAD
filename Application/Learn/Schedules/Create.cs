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
                // Cari Lesson berdasarkan LessonName
                var lesson = await _context.Lessons.SingleOrDefaultAsync(l => l.LessonName == request.ScheduleDto.LessonName);
                if (lesson == null)
                    return Result<ScheduleDto>.Failure("Lesson not found with the provided LessonName");

                // Cari ClassRoom berdasarkan ClassName
                var classRoom = await _context.ClassRooms.SingleOrDefaultAsync(cr => cr.ClassName == request.ScheduleDto.ClassName);
                if (classRoom == null)
                    return Result<ScheduleDto>.Failure("ClassRoom not found with the provided ClassName");

                // Cek apakah ClassRoom yang dipilih terkait dengan Lesson yang sudah dipilih sebelumnya
                var isClassRoomValidForLesson = await _context.LessonClassRooms
                    .AnyAsync(lc => lc.LessonId == lesson.Id && lc.ClassRoomId == classRoom.Id);

                if (!isClassRoomValidForLesson)
                    return Result<ScheduleDto>.Failure("The selected ClassRoom is not associated with the selected Lesson.");

                var schedule = new Schedule
                {
                    Day = request.ScheduleDto.Day,
                    StartTime = request.ScheduleDto.StartTime,
                    EndTime = request.ScheduleDto.EndTime,
                    LessonId = lesson.Id,
                    ClassRoomId = classRoom.Id
                };

                _context.Schedules.Add(schedule);

                var result = await _context.SaveChangesAsync(cancellationToken) > 0;

                if (!result) return Result<ScheduleDto>.Failure("Failed to create Schedule");

                // Buat DTO dari jadwal yang telah dibuat
                var scheduleDto = _mapper.Map<ScheduleDto>(schedule);

                // Tambahkan LessonName dan ClassName ke dalam response body
                scheduleDto.LessonName = lesson.LessonName;
                scheduleDto.ClassName = classRoom.ClassName;

                return Result<ScheduleDto>.Success(scheduleDto);
            }
        }
    }
}