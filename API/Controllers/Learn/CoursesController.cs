using Application.Learn.Courses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers.Learn
{
    public class CoursesController : BaseApiController
    {
        [Authorize(Policy = "RequireRole4")]
        [HttpGet]
        public async Task<IActionResult> GetCourses(CancellationToken ct)
        {
            return HandleResult(await Mediator.Send(new List.Query(), ct));
        }

        [Authorize(Policy = "RequireRole3")]
        [HttpGet("studentcourseclassroom")]
        public async Task<IActionResult> GetStudentCourses(CancellationToken ct)
        {
            return HandleResult(await Mediator.Send(new CourseStudentClassRoom.Query(), ct));
        }

        [Authorize(Policy = "RequireRole2")]
        [HttpGet("teachercourses")]
        public async Task<IActionResult> GetTeacherCourses(CancellationToken ct)
        {
            return HandleResult(await Mediator.Send(new CourseTeacher.Query(), ct));
        }


        [Authorize(Policy = "RequireRole2,3,4")]
        [HttpGet("{id}")]
        public async Task<ActionResult> GetCourse(Guid id, CancellationToken ct)
        {
            return HandleResult(await Mediator.Send(new Details.Query { Id = id }, ct));
        }

        [Authorize(Policy = "RequireRole2OrRole4")]
        [HttpPost]
        public async Task<IActionResult> Course([FromForm] CourseDto courseDto, CancellationToken ct)
        {
            // Ambil TeacherId dari token autentikasi
            var teacherId = Guid.Parse(HttpContext.User.FindFirst("TeacherId").Value);

            // Tetapkan TeacherId dari token autentikasi ke dalam CourseDto
            courseDto.TeacherId = teacherId;

            // Buat command menggunakan CourseDto yang diterima
            var command = new Create.Command { CourseDto = courseDto };

            // Kirim command ke handler untuk diproses
            return HandleResult(await Mediator.Send(command, ct));
        }

        [Authorize(Policy = "RequireRole2OrRole4")]
        [HttpPut("{id}")]
        public async Task<IActionResult> EditCourse(Guid id, [FromForm] CourseDto courseDto, CancellationToken ct)
        {
            // Ambil TeacherId dari token autentikasi
            var teacherId = Guid.Parse(HttpContext.User.FindFirst("TeacherId").Value);

            // Tetapkan TeacherId dari token autentikasi ke dalam CourseDto
            courseDto.TeacherId = teacherId;

            // Buat command menggunakan CourseDto yang diterima dan ID kursus yang diberikan
            var command = new EditCourse.Command { CourseId = id, CourseDto = courseDto };

            // Kirim command ke handler untuk diproses
            return HandleResult(await Mediator.Send(command, ct));
        }

        [Authorize(Policy = "RequireRole2,3,4")]
        [HttpGet("download/{id}")]
        public async Task<IActionResult> Download(Guid id, CancellationToken cancellationToken)
        {
            var result = await Mediator.Send(new Download.Query { Id = id }, cancellationToken);

            if (result.IsSuccess && result.Value != null)
            {
                var downloadFile = result.Value;
                var fileBytes = downloadFile.FileData;
                var contentType = downloadFile.ContentType;
                var fileName = downloadFile.FileName; // Nama file yang diunduh berdasarkan CourseName

                return File(fileBytes, contentType, fileName);
            }
            else
            {
                return NotFound(result.Error);
            }
        }

        [Authorize(Policy = "RequireRole2OrRole4")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteClassRoom(Guid id, CancellationToken ct)
        {
            return HandleResult(await Mediator.Send(new Delete.Command { Id = id }, ct));
        }
    }
}