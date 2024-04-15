﻿using Application.Attendances;
using Application.Attendances;
using Application.Attendances.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers.Attendances
{
    public class AttendancesController : BaseApiController
    {
        [Authorize(Policy = "RequireRole1,3,4")]
        [HttpGet]
        public async Task<IActionResult> GetAttendances(CancellationToken ct)
        {
            return HandleResult(await Mediator.Send(new List.Query(), ct));
        }

        [Authorize(Policy = "RequireRole1OrRole4")]
        [HttpGet("attendanceparam")]
        public async Task<ActionResult> GetAttendanceParam([FromQuery] int? year, [FromQuery] int? month, [FromQuery] int? day, [FromQuery] string nameStudent, [FromQuery] string className, CancellationToken ct)
        {
            return HandleResult(await Mediator.Send(new GetAttendanceByFilter.Query { Year = year, Month = month, Day = day, NameStudent = nameStudent, ClassName = className }, ct));
        }

        [Authorize(Policy = "RequireRole1OrRole4")]
        [HttpGet("{id}")]
        public async Task<ActionResult> GetAttendance(Guid id, CancellationToken ct)
        {
            return HandleResult(await Mediator.Send(new Details.Query { Id = id }, ct));
        }

        [Authorize(Policy = "RequireRole3OrRole4")]
        [HttpGet("student/{studentId}")]
        public async Task<ActionResult> GetAttendanceByStudentId(Guid studentId, CancellationToken ct)
        {
            return HandleResult(await Mediator.Send(new GetAllByStudentId.Query { StudentId = studentId }, ct));
        }

        [Authorize(Policy = "RequireRole1OrRole4")]
        [HttpPost]
        public async Task<IActionResult> CreateAttendanceDto(AttendanceDto announcementDto, CancellationToken ct)
        {
            return HandleResult(await Mediator.Send(new Create.Command { AttendanceDto = announcementDto }, ct));
        }

        [Authorize(Policy = "RequireRole1OrRole4")]
        [HttpPut("{id}")]
        public async Task<IActionResult> EditAttendanceDto(Guid id, AttendanceDto announcementDto, CancellationToken ct)
        {
            var result = await Mediator.Send(new Edit.Command { Id = id, AttendanceDto = announcementDto }, ct);

            return HandleResult(result);
        }

        [Authorize(Policy = "RequireRole1OrRole4")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAttendance(Guid id, CancellationToken ct)
        {
            return HandleResult(await Mediator.Send(new Delete.Command { Id = id }, ct));
        }
    }
}