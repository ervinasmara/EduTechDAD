using MediatR;
using Persistence;
using AutoMapper;
using FluentValidation;
using Application.Core;
using Domain.Class;
using Domain.Learn.Study;
using Microsoft.EntityFrameworkCore;

namespace Application.Learn.Study
{
    public class EditCoba1
    {
        public class Command : IRequest<Result<CourseEditDto>>
        {
            public Guid Id { get; set; }
            public CourseEditDto CourseEditDto { get; set; }
        }

        public class CommandValidatorDtos : AbstractValidator<Command>
        {
            public CommandValidatorDtos()
            {
                RuleFor(x => x.CourseEditDto).SetValidator(new CourseValidator());
            }
        }

        public class Handler : IRequestHandler<Command, Result<CourseEditDto>>
        {
            private readonly DataContext _context;
            private readonly IMapper _mapper;

            public Handler(DataContext context, IMapper mapper)
            {
                _context = context;
                _mapper = mapper;
            }

            public async Task<Result<CourseEditDto>> Handle(Command request, CancellationToken cancellationToken)
            {
                var courseEhe = await _context.Courses.FindAsync(request.Id);

                // Periksa apakah courseEhe ditemukan
                if (courseEhe == null)
                {
                    return Result<CourseEditDto>.Failure("Course Not Found");
                }

                // Membaca data file jika FileData tidak null
                byte[] fileData = null;
                if (request.CourseEditDto.FileData != null)
                {
                    using (var memoryStream = new MemoryStream())
                    {
                        await request.CourseEditDto.FileData.CopyToAsync(memoryStream);
                        fileData = memoryStream.ToArray();
                    }
                }

                // Memperbarui properti dari courseEhe dengan nilai yang diberikan dalam CourseEditDto
                _mapper.Map(request.CourseEditDto, courseEhe);

                // Menetapkan properti FileData dengan data file yang telah dikonversi menjadi byte[]
                courseEhe.FileData = fileData ?? courseEhe.FileData;

                // Menemukan Lesson terkait
                var lesson = await _context.Lessons.FirstOrDefaultAsync(x => x.UniqueNumber == request.CourseEditDto.UniqueNumber);
                if (lesson == null)
                {
                    return Result<CourseEditDto>.Failure("Lesson with specified UniqueNumber not found");
                }

                // Memperbarui UniqueNumber dengan nilai dari Lesson yang sesuai
                courseEhe.Lesson = lesson;

                var result = await _context.SaveChangesAsync(cancellationToken) > 0;

                if (!result)
                {
                    return Result<CourseEditDto>.Failure("Failed to edit Course");
                }

                // Buat instance CourseEditDto yang mewakili hasil edit
                var editedCourseEditDto = _mapper.Map<CourseEditDto>(courseEhe);

                return Result<CourseEditDto>.Success(editedCourseEditDto);
            }
        }
    }
}
