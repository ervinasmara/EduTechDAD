using Application.Core;
using System.Net;
using System.Text.Json;

namespace API.Middleware;
public class ExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionMiddleware> _logger;
    private readonly IHostEnvironment _env;
    public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger, IHostEnvironment env)
    {
        _env = env;
        _logger = logger;
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context); // Jadi begitu permintaan masuk, hal pertama yang akan dilakukan adalah meneruskannya ke middleware berikutnya
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message); // Pertama mengatakan LogError dan akan meneruskan message
            context.Response.ContentType = "application/json"; /* Kita menggunakan json ini karena kita tidak berada dalam Controller API
            Controller API akan secara default menggunakan json, tapi kita sedang berada di luar Controller, jadinya kita menentukan tipenya manual */
            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError; // Secara efektif ini akan menjadi 500

            var response = _env.IsDevelopment() // Menyimpan respon yang sama kemudian kita akan memeriksa env mana yang akan dijalankan
                ? new AppException(context.Response.StatusCode, ex.Message, ex.StackTrace?.ToString()) /* Kita akan menjadikan ini opsional "?" karena
                kita mungkin atau tidak memiliki StackTrace (pelacakan tumpukan) dan ini akan berisi format tambahan yang berguna*/
                : new AppException(context.Response.StatusCode, "Internal Server Error"); /* Dan jika kita tidak sedang dalam pengembangan (developer)
                kita akan membuat AppException (pengecualian aplikasi) */

            var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }; // Karena di luar Controller API, maka kita buat manual

            var json = JsonSerializer.Serialize(response, options); // Membuat respon json

            await context.Response.WriteAsync(json); // Kita akan meneruskan json sebagai apa yang akan kita kembalikan
        }
    }
}