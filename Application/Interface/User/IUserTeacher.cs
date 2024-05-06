using Application.Core;
using Application.User.DTOs.Registration;

namespace Application.Interface.User;
public interface IUserTeacher
{
    Task<Result<RegisterTeacherDto>> CreateTeacherAsync(RegisterTeacherDto teacherDto, CancellationToken cancellationToken);
}