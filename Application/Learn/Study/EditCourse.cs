using Application.ClassRooms;
using Application.Core;
using AutoMapper;
using Domain.Class;
using Domain.Learn.Study;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Http;
using Persistence;

namespace Application.Learn.Study
{
    public class EditCourse
    {
        public class Command : IRequest<Result<CourseEditDto>>
        {
            public Guid Id { get; set; }
            public CourseEditDto CourseEditDto { get; set; }
        }


        public class CommandValidatorDto : AbstractValidator<Command>
        {
            public CommandValidatorDto()
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
                _mapper = mapper;
                _context = context;
            }

            public async Task<Result<CourseEditDto>> Handle(Command request, CancellationToken cancellationToken)
            {
                var course = await _context.Courses.FindAsync(request.Id);

                // Periksa apakah course ditemukan
                if (course == null)
                {
                    return Result<CourseEditDto>.Failure("Course Not Found");
                }

                // Memetakan data dari Command ke entitas Course
                _mapper.Map(request, course);

                // Tidak perlu konversi data file

                var result = await _context.SaveChangesAsync(cancellationToken) > 0;

                if (!result)
                {
                    return Result<CourseEditDto>.Failure("Gagal untuk mengedit Course");
                }

                // Buat instance CourseEditDto yang mewakili hasil edit
                var editedCourseEditDto = _mapper.Map<CourseEditDto>(course);

                return Result<CourseEditDto>.Success(editedCourseEditDto);
            }
        }
    }
}