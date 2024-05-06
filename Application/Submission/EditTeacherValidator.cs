//using Application.Submission;
//using FluentValidation;

//namespace Application.Learn.Schedules
//{
//    public class EditTeacherValidator : AbstractValidator<AssignmentSubmissionTeacherDto>
//    {
//        public EditTeacherValidator()
//        {
//            RuleFor(x => x.Comment).NotEmpty();
//            RuleFor(x => x.Grade)
//            .NotEmpty()
//            .InclusiveBetween(0M, 100M) // Gunakan rentang dari 0 hingga 100 dengan tipe data decimal
//            .WithMessage("Grade must be a number between 0 and 100");
//        }
//    }
//}
