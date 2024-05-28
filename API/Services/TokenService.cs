using Domain.User;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace API.Services;
public class TokenService
{
    private readonly IConfiguration _config;

    public TokenService(IConfiguration config)
    {
        _config = config;
    }

    public string CreateTokenAdmin(AppUser user, Admin admin)
    {
        // Kita akan membuat daftar klaim yang akan masuk ke dalam dan dikembalikan dengan token kita
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Role, user.Role.ToString()),
            new Claim(ClaimTypes.NameIdentifier, user.Id),
            new Claim(ClaimTypes.Name, user.UserName),
            new Claim("NameAdmin", admin.NameAdmin),
        };

        // Dan kita perlu menggunakan kunci keamanan simetris
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["TokenKey"]));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature); // ini akan digunakan untuk menandatangani key

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddDays(2),
            SigningCredentials = creds
        };

        var tokenHandler = new JwtSecurityTokenHandler();

        var token = tokenHandler.CreateToken(tokenDescriptor);

        return tokenHandler.WriteToken(token);
    }

    public string CreateTokenTeacher(AppUser user, Teacher teacher)
    {
        // Kita akan membuat daftar klaim yang akan masuk ke dalam dan dikembalikan dengan token kita
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Role, user.Role.ToString()),
            new Claim(ClaimTypes.NameIdentifier, user.Id),
            new Claim(ClaimTypes.Name, user.UserName),
            new Claim("TeacherId", teacher.Id.ToString()),
            new Claim("NameTeacher", teacher.NameTeacher),
            new Claim("BirthDate", teacher.BirthDate.ToString()),
            new Claim("BirthPlace", teacher.BirthPlace),
            new Claim("Address", teacher.Address),
            new Claim("PhoneNumber", teacher.PhoneNumber),
            new Claim("Nip", teacher.Nip),
        };

        // Dan kita perlu menggunakan kunci keamanan simetris
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["TokenKey"]));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature); // ini akan digunakan untuk menandatangani key

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddDays(2),
            SigningCredentials = creds
        };

        var tokenHandler = new JwtSecurityTokenHandler();

        var token = tokenHandler.CreateToken(tokenDescriptor);

        return tokenHandler.WriteToken(token);
    }

    public string CreateTokenStudent(AppUser user, Student student)
    {
        // Mencari entitas ClassRoom yang terkait dengan siswa
        var className = student.ClassRoom?.ClassName ?? "Unknown"; // Jika ClassRoom null, maka ClassName diatur menjadi "Unknown"
        var classRoomId = student.ClassRoom.Id;

        // Kita akan membuat daftar klaim yang akan masuk ke dalam dan dikembalikan dengan token kita
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Role, user.Role.ToString()),
            new Claim(ClaimTypes.NameIdentifier, user.Id),
            new Claim(ClaimTypes.Name, user.UserName),
            new Claim("StudentId", student.Id.ToString()),
            new Claim("NameStudent", student.NameStudent),
            new Claim("BirthDate", student.BirthDate.ToString()),
            new Claim("BirthPlace", student.BirthPlace),
            new Claim("Address", student.Address),
            new Claim("PhoneNumber", student.PhoneNumber),
            new Claim("Nis", student.Nis),
            new Claim("ParentName", student.ParentName),
            new Claim("Gender", student.Gender.ToString()),
            new Claim("ClassRoomId", classRoomId.ToString()), // Ubah classRoomId menjadi string sebelum menambahkannya sebagai klaim
            new Claim("ClassName", className),
        };

        // Dan kita perlu menggunakan kunci keamanan simetris
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["TokenKey"]));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature); // ini akan digunakan untuk menandatangani key

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddDays(2),
            SigningCredentials = creds
        };

        var tokenHandler = new JwtSecurityTokenHandler();

        var token = tokenHandler.CreateToken(tokenDescriptor);

        return tokenHandler.WriteToken(token);
    }

}