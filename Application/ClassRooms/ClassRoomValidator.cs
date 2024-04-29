using Domain.Class;
using FluentValidation;

namespace Application.ClassRooms
{
    public class ClassRoomValidator : AbstractValidator<ClassRoomCreateAndEditDto>
    {
        public ClassRoomValidator()
        {
            RuleFor(x => x.ClassName).NotEmpty();
            RuleFor(x => x.LongClassName).NotEmpty();
        }
    }
}
