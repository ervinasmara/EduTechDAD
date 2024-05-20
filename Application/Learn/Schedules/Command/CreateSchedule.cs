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
                    .Include(l => l.ClassRoom)
                    .FirstOrDefaultAsync(l => l.LessonName == request.ScheduleCreateAndEditDto.LessonName, cancellationToken);

                /** Langkah 2: Memeriksa Ketersediaan Pelajaran **/
                if (lesson == null)
                    return Result<ScheduleCreateAndEditDto>.Failure($"Pelajaran dengan nama '{request.ScheduleCreateAndEditDto.LessonName}' tidak ditemukan");

                /** Langkah 3: Memeriksa Apakah Jadwal Bertumpuk dengan Jadwal yang Ada dalam Kelas yang Sama **/
                var existingSchedules = await _context.Schedules
                    .Where(s => s.Lesson.ClassRoomId == lesson.ClassRoomId && s.Day == request.ScheduleCreateAndEditDto.Day)
                    .ToListAsync(cancellationToken);

                var newStartTime = request.ScheduleCreateAndEditDto.StartTime;
                var newEndTime = request.ScheduleCreateAndEditDto.EndTime;
                var dayName = GetDayName(request.ScheduleCreateAndEditDto.Day);

                foreach (var schedule in existingSchedules)
                {
                    if ((newStartTime >= schedule.StartTime && newStartTime < schedule.EndTime) ||
                        (newEndTime > schedule.StartTime && newEndTime <= schedule.EndTime) ||
                        (newStartTime <= schedule.StartTime && newEndTime >= schedule.EndTime))
                    {
                        return Result<ScheduleCreateAndEditDto>.Failure(
                            $"Jadwal sudah ada pada hari {dayName} " +
                            $"pada jam {schedule.StartTime:hh\\:mm} - {schedule.EndTime:hh\\:mm} di kelas {lesson.ClassRoom.ClassName}");
                    }
                }

                /** Langkah 4: Membuat Instance Jadwal dari ScheduleCreateAndEditDto dan Mengatur LessonId **/
                var newSchedule = _mapper.Map<Schedule>(request.ScheduleCreateAndEditDto);
                newSchedule.LessonId = lesson.Id;

                /** Langkah 5: Menambahkan Jadwal ke Database **/
                _context.Schedules.Add(newSchedule);

                /** Langkah 6: Menyimpan Perubahan ke Database **/
                var result = await _context.SaveChangesAsync(cancellationToken) > 0;

                /** Langkah 7: Memeriksa Hasil Simpan **/
                if (!result)
                    return Result<ScheduleCreateAndEditDto>.Failure("Gagal untuk membuat jadwal");

                /** Langkah 8: Mengembalikan Hasil dalam Bentuk Success Result **/
                var scheduleDto = _mapper.Map<ScheduleCreateAndEditDto>(newSchedule);

                return Result<ScheduleCreateAndEditDto>.Success(scheduleDto);
            }
            catch (Exception ex)
            {
                /** Langkah 9: Menangani Kesalahan Jika Terjadi **/
                return Result<ScheduleCreateAndEditDto>.Failure($"Gagal untuk membuat jadwal: {ex.Message}");
            }
        }

        private static string GetDayName(int day)
        {
            return day switch
            {
                1 => "Senin",
                2 => "Selasa",
                3 => "Rabu",
                4 => "Kamis",
                5 => "Jumat",
                6 => "Sabtu",
                7 => "Minggu",
                _ => "Tidak diketahui"
            };
        }
    }
}