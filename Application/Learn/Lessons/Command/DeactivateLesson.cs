using Application.Core;
using Application.Interface;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Learn.Lessons.Command
{
    public class DeactivateLesson
    {
        public class Command : IRequest<Result<object>>
        {
            public Guid Id { get; set; }
        }

        public class Validator : AbstractValidator<Command>
        {
            public Validator()
            {
                RuleFor(x => x.Id).NotEmpty();
            }
        }

        public class EditLessonStatusHandler : IRequestHandler<Command, Result<object>>
        {
            private readonly DataContext _context;
            private readonly IUserAccessor _userAccessor;

            public EditLessonStatusHandler(DataContext context, IUserAccessor userAccessor)
            {
                _context = context;
                _userAccessor = userAccessor;
            }

            public async Task<Result<object>> Handle(Command request, CancellationToken cancellationToken)
            {
                /** Langkah 1: Ubah status Lesson menjadi 0 **/
                var lesson = await _context.Lessons
                    .FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken);

                // Periksa apakah ada lesson yang ditemukan
                if (lesson == null)
                {
                    return Result<object>.Failure("Lesson not found");
                }

                /** Langkah 2: Mengubah status lesson menjadi 0 (nonaktif) **/
                lesson.Status = 0;

                /** Langkah 3: Simpan perubahan status ke database **/
                await _context.SaveChangesAsync(cancellationToken);

                /** Langkah 4: Mengembalikan hasil berhasil dengan pesan **/
                return Result<object>.Success(new { Message = "Lesson status updated successfully" });
            }
        }
    }
}
