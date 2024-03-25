using Domain.Class;
using FluentValidation;

namespace Application.ClassRooms
{
    public class ClassRoomValidator : AbstractValidator<ClassRoomDto>
    {
        public ClassRoomValidator()
        {
            RuleFor(x => x.ClassName).NotEmpty();
            RuleFor(x => x.UniqueNumber).NotEmpty();
        }
    }
}