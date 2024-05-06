namespace Application.User.DTOs;
public class AdminDto
{
    public int Role { get; set; }
    public string Username { get; set; }
    public string NameAdmin { get; set; }
    public string Token { get; set; }
}

public class AdminGetDto
{
    public int Role { get; set; }
    public string Username { get; set; }
    public string NameAdmin { get; set; }
}