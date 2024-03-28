using Application.Learn.Study;
using Domain.Class;
using Domain.Learn.Study;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers.Learn
{
    [AllowAnonymous]
    public class CoursesController : BaseApiController
    {
        [HttpGet]
        public async Task<IActionResult> GetCourses(CancellationToken ct)
        {
            return HandleResult(await Mediator.Send(new List.Query(), ct));
        }

        [HttpPost]
        public async Task<IActionResult> Course([FromForm] Create.Command command, CancellationToken ct)
        {
            return HandleResult(await Mediator.Send(command, ct));
        }

        [HttpGet("download/{id}")]
        public async Task<IActionResult> Download(Guid id, CancellationToken cancellationToken)
        {
            var result = await Mediator.Send(new Download.Query { Id = id }, cancellationToken);

            if (result.IsSuccess && result.Value != null)
            {
                var downloadFile = result.Value;
                var fileBytes = downloadFile.FileData;
                var contentType = "application/octet-stream";
                var fileName = downloadFile.FileName; // Nama file yang diunduh berdasarkan CourseName

                return File(fileBytes, contentType, fileName);
            }
            else
            {
                return NotFound(result.Error);
            }
        }

        //[HttpPut("{id}")]
        //public async Task<IActionResult> EditCourseDto([FromForm] Edit.Command command, CancellationToken ct)
        //{
        //    return HandleResult(await Mediator.Send(command, ct));
        //}

        //[HttpPut("2/{id}")]
        //public async Task<IActionResult> EditCourseDto2(Guid id, [FromForm] CourseDto courseDto, CancellationToken ct)
        //{
        //    // Manual mapping dari CourseDto ke Edit.Command
        //    var command = new Edit.Command
        //    {
        //        Id = id,
        //        CourseName = courseDto.CourseName,
        //        Description = courseDto.Description,
        //        FileData = null, // Anda perlu memutuskan bagaimana menangani ini
        //        LinkCourse = courseDto.LinkCourse,
        //        UniqueNumber = courseDto.UniqueNumber
        //    };

        //    // Kirim command ke handler menggunakan Mediator
        //    var result = await Mediator.Send(command, ct);
        //    return HandleResult(result);
        //}

        //[HttpPut("haha/{id}")]
        //public async Task<IActionResult> EditCourseEditDto(Guid id, [FromForm] CourseEditDto courseEditDto, CancellationToken ct)
        //{
        //    var result = await Mediator.Send(new EditCourse.Command { Id = id, CourseEditDto = courseEditDto }, ct);

        //    return HandleResult(result);
        //}

        //[HttpPut("edit/{id}")]
        //public async Task<IActionResult> EditCourse(Guid id, [FromForm] EditCourse18.Command command)
        //{
        //    command.Id = id;
        //    var result = await Mediator.Send(command);

        //    if (result.IsSuccess)
        //    {
        //        return Ok(result.Value); // Jika berhasil, kembalikan data CourseDto yang telah diubah
        //    }

        //    return BadRequest(result.Error); // Jika gagal, kembalikan pesan kesalahan
        //}

        [HttpPut("coba1/{id}")]
        public async Task<IActionResult> EditCourseDtoss(Guid id,[FromForm] CourseEditDto courseEditDto, CancellationToken ct)
        {
            var result = await Mediator.Send(new EditCoba1.Command { Id = id, CourseEditDto = courseEditDto }, ct);

            return HandleResult(result);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteClassRoom(Guid id, CancellationToken ct)
        {
            return HandleResult(await Mediator.Send(new Delete.Command { Id = id }, ct));
        }
    }
}