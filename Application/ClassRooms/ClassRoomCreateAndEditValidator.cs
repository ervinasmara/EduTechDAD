using FluentValidation;

namespace Application.ClassRooms;
public class ClassRoomCreateAndEditValidator : AbstractValidator<ClassRoomCreateAndEditDto>
{
    public ClassRoomCreateAndEditValidator()
    {
        RuleFor(x => x.ClassName).NotEmpty().WithMessage("Nama kelas tidak boleh kosong");
        RuleFor(x => x.LongClassName).NotEmpty().WithMessage("Nama lengkap kelas tidak boleh kosong");
    }
}