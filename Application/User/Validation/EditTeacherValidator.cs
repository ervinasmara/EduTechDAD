using Application.User.DTOs.Edit;
using FluentValidation;

namespace Application.User.Validation
{
    public class EditTeacherValidator : AbstractValidator<EditTeacherDto>
    {
        public EditTeacherValidator()
        {
            RuleFor(x => x.Address).NotEmpty().WithMessage("Address is required.");
            RuleFor(x => x.PhoneNumber).NotEmpty().WithMessage("Phone number is required.")
                                          .Matches("^[0-9]{8,13}$").WithMessage("Phone number must be between 8 and 13 digits and contain only numbers.");
            RuleFor(x => x.LessonNames).NotEmpty().WithMessage("LessonNames is required.");
        }
    }
}
