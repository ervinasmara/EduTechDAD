using FluentValidation;

namespace Application.ClassRooms;
public class ClassRoomCreateAndEditValidator : AbstractValidator<ClassRoomCreateAndEditDto>
{
    public ClassRoomCreateAndEditValidator()
    {
        RuleFor(x => x.ClassName).NotEmpty();
        RuleFor(x => x.LongClassName).NotEmpty();
    }
}