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
        public class Command : IRequest<Result<ScheduleDto>>
        {
            public Guid Id { get; set; } // ID dari jadwal yang ingin diubah
            public ScheduleDto ScheduleDto { get; set; } // Data jadwal yang baru
        }

        public class CommandValidator : AbstractValidator<Command>
        {
            public CommandValidator()
            {
                RuleFor(x => x.Id).NotEmpty();
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
                // Cari jadwal yang akan diubah berdasarkan ID
                var schedule = await _context.Schedules.FindAsync(request.Id);

                if (schedule == null)
                    return Result<ScheduleDto>.Failure("Schedule not found");

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

                // Update informasi jadwal
                schedule.Day = request.ScheduleDto.Day;
                schedule.StartTime = request.ScheduleDto.StartTime;
                schedule.EndTime = request.ScheduleDto.EndTime;
                schedule.LessonId = lesson.Id;
                schedule.ClassRoomId = classRoom.Id;

                // Simpan perubahan ke dalam database
                var result = await _context.SaveChangesAsync(cancellationToken) > 0;

                if (!result)
                    return Result<ScheduleDto>.Failure("Failed to update Schedule");

                // Buat DTO dari jadwal yang telah diubah
                var scheduleDto = _mapper.Map<ScheduleDto>(schedule);

                // Tambahkan LessonName dan ClassName ke dalam response body
                scheduleDto.LessonName = lesson.LessonName;
                scheduleDto.ClassName = classRoom.ClassName;

                return Result<ScheduleDto>.Success(scheduleDto);
            }
        }
    }
}