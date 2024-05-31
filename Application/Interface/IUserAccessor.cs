namespace Application.Interface;
public interface IUserAccessor
{
    string GetTeacherIdFromToken();
    string GetNameTeacherFromToken();
    string GetClassRoomIdFromToken();
    string GetStudentIdFromToken();
}