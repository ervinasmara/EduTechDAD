using Application.Core;
using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Learn.Courses
{
    public class Details
    {
        public class Query : IRequest<Result<CourseGetDto>> // Mengubah DTO menjadi CourseGetDto
        {
            public Guid Id { get; set; }
        }

        public class Handler : IRequestHandler<Query, Result<CourseGetDto>> // Mengubah Course menjadi CourseGetDto
        {
            private readonly DataContext _context;
            private readonly IMapper _mapper;

            public Handler(DataContext context, IMapper mapper)
            {
                _context = context;
                _mapper = mapper;
            }

            public async Task<Result<CourseGetDto>> Handle(Query request, CancellationToken cancellationToken)
            {
                var course = await _context.Courses
                                            .Include(c => c.Lesson)
                                            .FirstOrDefaultAsync(c => c.Id == request.Id);

                if (course == null)
                    return Result<CourseGetDto>.Failure("Course not found.");

                var courseDto = _mapper.Map<CourseGetDto>(course); // Memetakan Course ke CourseGetDto

                // Set FileName based on CourseName and FileData extension
                if (!string.IsNullOrEmpty(course.CourseName) && course.FileData != null)
                {
                    courseDto.FileName = $"{course.CourseName}.{GetFileExtension(course.FileData)}";
                }
                else
                {
                    // Handle null values appropriately
                    courseDto.FileName = "UnknownFileName";
                }

                // Set UniqueNumberOfLesson from Lesson entity
                if (course.Lesson != null)
                {
                    courseDto.UniqueNumberOfLesson = course.Lesson.UniqueNumberOfLesson;
                }
                else
                {
                    // Handle null values appropriately
                    courseDto.UniqueNumberOfLesson = "UnknownUniqueNumberOfLesson";
                }

                return Result<CourseGetDto>.Success(courseDto);
            }


            private string GetFileExtension(byte[] fileData)
            {
                if (fileData == null || fileData.Length < 4)
                    return null;

                // Analisis byte pertama untuk menentukan jenis file
                if (fileData[0] == 0xFF && fileData[1] == 0xD8 && fileData[2] == 0xFF)
                {
                    return "jpg";
                }
                else if (fileData[0] == 0x89 && fileData[1] == 0x50 && fileData[2] == 0x4E && fileData[3] == 0x47)
                {
                    return "png";
                }
                else if (fileData[0] == 0x25 && fileData[1] == 0x50 && fileData[2] == 0x44 && fileData[3] == 0x46)
                {
                    return "pdf";
                }
                else if (fileData[0] == 0x50 && fileData[1] == 0x4B && fileData[2] == 0x03 && fileData[3] == 0x04)
                {
                    return "zip";
                }
                else if (fileData[0] == 0x52 && fileData[1] == 0x61 && fileData[2] == 0x72 && fileData[3] == 0x21)
                {
                    return "rar";
                }
                else
                {
                    return null; // Ekstensi file tidak dikenali
                }
            }
        }
    }
}
