using Application.Learn.Study;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers.Learn
{
    [AllowAnonymous]
    public class CoursesController : BaseApiController
    {
        //[HttpGet]
        //public async Task<IActionResult> GetCourses(CancellationToken ct)
        //{
        //    return HandleResult(await Mediator.Send(new List.Query(), ct));
        //}

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
                var contentType = "application/pdf";
                var fileName = downloadFile.FileName; // Nama file yang diunduh berdasarkan CourseName

                return File(fileBytes, contentType, fileName);
            }
            else
            {
                return NotFound(result.Error);
            }
        }

    }
}