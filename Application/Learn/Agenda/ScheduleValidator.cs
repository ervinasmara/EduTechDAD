using Domain.Learn.Agenda;
using FluentValidation;

namespace Application.Learn.Agenda
{
    public class ScheduleValidator : AbstractValidator<ScheduleDto>
    {
        public ScheduleValidator()
        {
            RuleFor(x => x.Day)
               .NotEmpty()
               .InclusiveBetween(1, 7).WithMessage("Day must be between 1 (Monday) and 7 (Sunday).");

            RuleFor(x => x.StartTime).NotEmpty();

            RuleFor(x => x.EndTime).NotEmpty()
                .GreaterThan(x => x.StartTime)
                .WithMessage("EndTime must be greater than StartTime");

            RuleFor(x => x.LessonId).NotEmpty();
            RuleFor(x => x.ClassRoomId).NotEmpty();
        }
    }
}
