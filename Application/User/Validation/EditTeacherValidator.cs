using Application.User.DTOs.Edit;
using Application.User.DTOs.Registration;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
            RuleFor(x => x.ClassNames).NotEmpty().WithMessage("ClassNames is required.");

        }
    }
}
