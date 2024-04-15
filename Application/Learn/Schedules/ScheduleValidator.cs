using FluentValidation;

namespace Application.Learn.Schedules
{
    public class ScheduleValidator : AbstractValidator<ScheduleDto>
    {
        public ScheduleValidator()
        {
            RuleFor(x => x.Day)
               .NotEmpty()
               .InclusiveBetween(1, 5).WithMessage("Day must be between 1 (Monday) and 5 (Friday).");

            RuleFor(x => x.StartTime).NotEmpty();

            RuleFor(x => x.EndTime).NotEmpty()
                .GreaterThan(x => x.StartTime)
                .WithMessage("EndTime must be greater than StartTime");

            RuleFor(x => x.UniqueNumberOfLesson).NotEmpty();
            RuleFor(x => x.UniqueNumberOfClassRoom).NotEmpty();
        }
    }
}
