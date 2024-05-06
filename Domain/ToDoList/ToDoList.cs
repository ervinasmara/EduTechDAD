namespace Domain.ToDoList;
public class ToDoList
{
    public Guid Id { get; set; }
    public string Description { get; set; }
    public int Status { get; set; }
    public DateTime CreatedAt { get; set; }
}