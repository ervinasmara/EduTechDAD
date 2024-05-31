using Application.Interface;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace Infrastructure.Security;
public class UserAccessor : IUserAccessor
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public UserAccessor(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public string GetTeacherIdFromToken()
    {
        var userId = _httpContextAccessor.HttpContext.User?.FindFirstValue("TeacherId");
        return userId;
    }

    public string GetNameTeacherFromToken()
    {
        var nameTeacher = _httpContextAccessor.HttpContext.User?.FindFirstValue("NameTeacher");
        return nameTeacher;
    }

    public string GetClassRoomIdFromToken()
    {
        var userId = _httpContextAccessor.HttpContext.User?.FindFirstValue("ClassRoomId");
        return userId;
    }

    public string GetStudentIdFromToken()
    {
        var userId = _httpContextAccessor.HttpContext.User?.FindFirstValue("StudentId");
        return userId;
    }
}