using Application.Core;
using Application.User.DTOs.Edit;
using Application.User.Validation;
using Domain.Many_to_Many;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.User.Teachers.Command
{
    public class EditTeacher
    {
        public class EditTeacherCommand : IRequest<Result<EditTeacherDto>>
        {
            public Guid TeacherId { get; set; }
            public EditTeacherDto TeacherDto { get; set; }
        }

        public class EditTeacherCommandValidator : AbstractValidator<EditTeacherCommand>
        {
            public EditTeacherCommandValidator()
            {
                RuleFor(x => x.TeacherDto).SetValidator(new EditTeacherValidator());
            }
        }

        public class EditTeacherCommandHandler : IRequestHandler<EditTeacherCommand, Result<EditTeacherDto>>
        {
            private readonly DataContext _context;

            public EditTeacherCommandHandler(DataContext context)
            {
                _context = context;
            }

            public async Task<Result<EditTeacherDto>> Handle(EditTeacherCommand request, CancellationToken cancellationToken)
            {
                var teacher = await _context.Teachers
                    .Include(t => t.TeacherLessons)
                    .ThenInclude(tl => tl.Lesson)
                    .FirstOrDefaultAsync(t => t.Id == request.TeacherId);

                if (teacher == null)
                {
                    return Result<EditTeacherDto>.Failure("Teacher not found");
                }

                teacher.Address = request.TeacherDto.Address;
                teacher.PhoneNumber = request.TeacherDto.PhoneNumber;

                // Hapus semua relasi pelajaran yang ada
                _context.TeacherLessons.RemoveRange(teacher.TeacherLessons);

                // Tambahkan kembali relasi pelajaran yang baru
                foreach (var lessonName in request.TeacherDto.LessonNames)
                {
                    var lesson = await _context.Lessons.FirstOrDefaultAsync(l => l.LessonName == lessonName);
                    if (lesson != null)
                    {
                        teacher.TeacherLessons.Add(new TeacherLesson { LessonId = lesson.Id });
                    }
                }

                var result = await _context.SaveChangesAsync(cancellationToken) > 0;

                if (!result)
                {
                    return Result<EditTeacherDto>.Failure("Failed to update teacher");
                }

                var editedTeacherDto = new EditTeacherDto
                {
                    Address = teacher.Address,
                    PhoneNumber = teacher.PhoneNumber,
                    LessonNames = teacher.TeacherLessons.Select(tl => tl.Lesson.LessonName).ToList()
                };

                return Result<EditTeacherDto>.Success(editedTeacherDto);
            }
        }
    }
}