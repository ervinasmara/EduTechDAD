using FluentValidation;

namespace Application.Learn.Schedules;
public class ScheduleCreateAndEditValidator : AbstractValidator<ScheduleCreateAndEditDto>
{
    public ScheduleCreateAndEditValidator()
    {
        RuleFor(x => x.Day)
            .NotEmpty()
            .InclusiveBetween(1, 5).WithMessage("Hari harus antara 1 (Senin) dan 5 (Jumat).");

        RuleFor(x => x.StartTime).NotEmpty().WithMessage("Waktu mulai tidak boleh kosong");

        RuleFor(x => x.EndTime).NotEmpty()
            .GreaterThan(x => x.StartTime)
            .WithMessage("Akhir waktu harus lebih besar dari waktu mulai");

        RuleFor(x => x.LessonName).NotEmpty().WithMessage("Nama pelajaran tidak boleh kosong");
    }
}