using Application.Core;
using AutoMapper;
using Domain.Learn.Lessons;
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
                    .Include(l => l.TeacherLessons)
                        .ThenInclude(tl => tl.Teacher)
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
                            $"pada jam {schedule.StartTime:hh\\:mm\\:dd} - {schedule.EndTime:hh\\:mm\\:dd} di kelas {lesson.ClassRoom.ClassName}");
                    }
                }

                /** Langkah 4: Memeriksa Apakah Jadwal Bertumpuk dengan Jadwal yang Ada untuk Lesson yang Sama pada Hari yang Berbeda **/
                var lessonId = lesson.Id;
                var scheduleExist = await _context.Schedules
                .Include(s => s.Lesson)
                    .Where(s => s.LessonId == lessonId)
                    .ToListAsync(cancellationToken);

                foreach (var schedule in scheduleExist)
                {
                    if ((newStartTime >= schedule.StartTime && newStartTime < schedule.EndTime) ||
                        (newEndTime > schedule.StartTime && newEndTime <= schedule.EndTime) ||
                        (newStartTime <= schedule.StartTime && newEndTime >= schedule.EndTime))
                    {
                        return Result<ScheduleCreateAndEditDto>.Failure(
                            $"Jadwal untuk mapel ini sudah tersedia pada hari {GetDayName(schedule.Day)} " +
                            $"pada jam {schedule.StartTime:hh\\:mm\\:ss} - {schedule.EndTime:hh\\:mm\\:ss} di kelas {schedule.Lesson.ClassRoom.ClassName}");
                    }
                }

                /** Langkah 5: Membuat Instance Jadwal dari ScheduleCreateAndEditDto dan Mengatur LessonId **/
                var newSchedule = _mapper.Map<Schedule>(request.ScheduleCreateAndEditDto);
                newSchedule.LessonId = lesson.Id;

                /** Langkah 6: Menambahkan Jadwal ke Database **/
                _context.Schedules.Add(newSchedule);

                /** Langkah 7: Menyimpan Perubahan ke Database **/
                var result = await _context.SaveChangesAsync(cancellationToken) > 0;

                /** Langkah 8: Memeriksa Hasil Simpan **/
                if (!result)
                    return Result<ScheduleCreateAndEditDto>.Failure("Gagal untuk membuat jadwal");

                /** Langkah 9: Mengembalikan Hasil dalam Bentuk Success Result **/
                var scheduleDto = _mapper.Map<ScheduleCreateAndEditDto>(newSchedule);

                return Result<ScheduleCreateAndEditDto>.Success(scheduleDto);
            }
            catch (Exception ex)
            {
                /** Langkah 10: Menangani Kesalahan Jika Terjadi **/
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