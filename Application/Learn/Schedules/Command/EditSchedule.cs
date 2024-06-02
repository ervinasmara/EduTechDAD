﻿using Application.Core;
using AutoMapper;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Learn.Schedules.Command;
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
                /** Langkah 1: Mencari Jadwal yang Akan Diedit **/
                var schedule = await _context.Schedules.FindAsync(request.ScheduleId);

                /** Langkah 2: Memeriksa Ketersediaan Jadwal yang Akan Diedit **/
                if (schedule == null)
                    return Result<ScheduleCreateAndEditDto>.Failure($"Jadwal dengan id '{request.ScheduleId}' tidak ditemukan");

                /** Langkah 3: Memetakan Data yang Diperbarui dari DTO ke Entitas Schedule menggunakan AutoMapper **/
                _mapper.Map(request.ScheduleCreateAndEditDto, schedule);

                /** Langkah 4: Mencari Pelajaran Berdasarkan Nama Pelajaran yang Diberikan **/
                var lesson = await _context.Lessons
                    .FirstOrDefaultAsync(l => l.LessonName == request.ScheduleCreateAndEditDto.LessonName, cancellationToken);

                /** Langkah 5: Memeriksa Ketersediaan Pelajaran **/
                if (lesson == null)
                    return Result<ScheduleCreateAndEditDto>.Failure($"Pelajaran dengan nama '{request.ScheduleCreateAndEditDto.LessonName}' tidak ditemukan");

                // Menetapkan LessonId yang sesuai ke Schedule yang sedang diedit
                schedule.LessonId = lesson.Id;

                var overlappingSchedule = await _context.Schedules
                .Where(s => s.Lesson.ClassRoomId == lesson.ClassRoomId && s.Day == request.ScheduleCreateAndEditDto.Day)
                .Select(s => new {
                    s.Id,
                    s.Day,
                    StartTime = s.StartTime,
                    EndTime = s.EndTime,
                    s.Lesson.ClassRoom.ClassName
                })
                .FirstOrDefaultAsync(s => (request.ScheduleCreateAndEditDto.StartTime < s.EndTime && request.ScheduleCreateAndEditDto.EndTime > s.StartTime) && s.Id != request.ScheduleId, cancellationToken);

                if (overlappingSchedule != null)
                {
                    var dayName = GetDayName((int)overlappingSchedule.Day);
                    var startTime = overlappingSchedule.StartTime.ToString(@"hh\:mm\:dd");
                    var endTime = overlappingSchedule.EndTime.ToString(@"hh\:mm\:dd");
                    return Result<ScheduleCreateAndEditDto>.Failure(
                        $"Jadwal sudah ada pada hari {dayName} pada jam {startTime} - {endTime} di kelas {overlappingSchedule.ClassName}");
                }

                /** Langkah 7: Menyimpan Perubahan ke Database **/
                var result = await _context.SaveChangesAsync(cancellationToken) > 0;

                /** Langkah 8: Memeriksa Hasil Simpan **/
                if (!result)
                    return Result<ScheduleCreateAndEditDto>.Failure("Gagal untuk mengedit jadwal");

                /** Langkah 9: Membuat DTO Respons dan Mengembalikan **/
                var scheduleDto = _mapper.Map<ScheduleCreateAndEditDto>(schedule);
                scheduleDto.LessonName = lesson.LessonName; // Set LessonName in response

                return Result<ScheduleCreateAndEditDto>.Success(scheduleDto);
            }
            catch (Exception ex)
            {
                /** Langkah 10: Menangani Kesalahan Jika Terjadi **/
                return Result<ScheduleCreateAndEditDto>.Failure($"Gagal untuk mengedit jadwal: {ex.Message}");
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