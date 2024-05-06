namespace Application.ClassRooms;
/** ClassRoom Get All and By Id **/
public class ClassRoomGetDto
{
    public Guid Id { get; set; }
    public Guid ClassRoomId { get; set; }
    public string ClassName { get; set; }
    public string LongClassName { get; set; }
    public string UniqueNumberOfClassRoom { get; set; }
}

/** Create and Edit ClassRoom **/
public class ClassRoomCreateAndEditDto
{
    public string ClassName { get; set; }
    public string LongClassName { get; set; }
}

/** ClassRoom Get For Teacher **/
public class ClassRoomDto
{
    public string ClassName { get; set; }
    public string LongClassName { get; set; }
    public string UniqueNumberOfClassRoom { get; set; }
}
public class ClassRoomTeacherDto
{
    public string TeacherId { get; set; }
    public List<ClassRoomDto> ClassRooms { get; set; }
}