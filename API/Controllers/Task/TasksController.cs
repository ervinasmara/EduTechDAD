﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Application.Tasks;
using Domain.Task;
using Domain.Announcement;

namespace API.Controllers.Tasks
{
    [AllowAnonymous]
    public class AssignmentsController : BaseApiController
    {
        [HttpGet]
        public async Task<IActionResult> GetAsignments(CancellationToken ct)
        {
            return HandleResult(await Mediator.Send(new ListTasks.Query(), ct));
        }

        [HttpPost]
        public async Task<IActionResult> CreateTasks([FromForm] CreateTask.Command command, CancellationToken ct)
        {
            return HandleResult(await Mediator.Send(command, ct));
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> EditAssignment(Guid id, [FromForm] AssignmentDto assignmentDto)
        {
            var command = new EditTask.Command
            {
                Id = id,
                AssignmentDto = assignmentDto
            };

            var result = await Mediator.Send(command);

            if (result.IsSuccess)
            {
                var editedAssignment = result.Value;
                return Ok(editedAssignment); // Mengembalikan respons 200 OK dengan data assignment yang berhasil diubah
            }
            else
            {
                return BadRequest(result.Error); // Mengembalikan respons 400 Bad Request dengan pesan kesalahan jika edit gagal
            }
        }

        [HttpGet("download/{id}")]
        public async Task<IActionResult> DownloadTask(Guid id, CancellationToken cancellationToken)
        {
            var result = await Mediator.Send(new DownloadTask.Query { Id = id }, cancellationToken);

            if (result.IsSuccess && result.Value != null)
            {
                var downloadFile = result.Value;
                var fileBytes = downloadFile.FileData;
                var contentType = "application/octet-stream";
                var fileName = downloadFile.FileName; // Nama file yang diunduh berdasarkan AssignmentName

                return File(fileBytes, contentType, fileName);
            }
            else
            {
                return NotFound(result.Error);
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteClassRoom(Guid id, CancellationToken ct)
        {
            return HandleResult(await Mediator.Send(new DeleteTask.Command { Id = id }, ct));
        }
    }
}