namespace Application.Interface;
public interface IUserAccessor
{
    string GetTeacherIdFromToken();
    string GetClassRoomIdFromToken();
    string GetStudentIdFromToken();
}