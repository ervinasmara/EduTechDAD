//using MediatR;
//using Persistence;
//using AutoMapper;
//using FluentValidation;
//using Application.Core;
//using Microsoft.EntityFrameworkCore;
//using Domain.Many_to_Many;

//namespace Application.Learn.Courses
//{
//    public class Edit
//    {
//        public class Command : IRequest<Result<CourseDto>>
//        {
//            public Guid Id { get; set; }
//            public CourseDto CourseDto { get; set; }
//        }

//        public class CommandValidatorDtos : AbstractValidator<Command>
//        {
//            public CommandValidatorDtos()
//            {
//                RuleFor(x => x.CourseDto).SetValidator(new CourseValidator());
//            }
//        }

//        public class Handler : IRequestHandler<Command, Result<CourseDto>>
//        {
//            private readonly DataContext _context;
//            private readonly IMapper _mapper;

//            public Handler(DataContext context, IMapper mapper)
//            {
//                _context = context;
//                _mapper = mapper;
//            }

//            public async Task<Result<CourseDto>> Handle(Command request, CancellationToken cancellationToken)
//            {
//                var course = await _context.Courses
//                    .Include(c => c.CourseClassRooms)
//                    .FirstOrDefaultAsync(c => c.Id == request.Id);

//                // Periksa apakah course ditemukan
//                if (course == null)
//                {
//                    return Result<CourseDto>.Failure("Course Not Found");
//                }

//                // Membaca data file jika FileData tidak null
//                byte[] fileData = null;
//                if (request.CourseDto.FileData != null)
//                {
//                    using (var memoryStream = new MemoryStream())
//                    {
//                        await request.CourseDto.FileData.CopyToAsync(memoryStream);
//                        fileData = memoryStream.ToArray();
//                    }
//                }

//                // Memperbarui properti dari course dengan nilai yang diberikan dalam CourseDto
//                _mapper.Map(request.CourseDto, course);

//                // Menetapkan properti FileData dengan data file yang telah dikonversi menjadi byte[]
//                course.FileData = fileData ?? course.FileData;

//                // Menemukan Lesson terkait
//                var lesson = await _context.Lessons.FirstOrDefaultAsync(x => x.UniqueNumberOfLesson == request.CourseDto.UniqueNumberOfLesson);
//                if (lesson == null)
//                {
//                    return Result<CourseDto>.Failure("Lesson with specified UniqueNumberOfLesson not found");
//                }

//                // Memperbarui UniqueNumberOfLesson dengan nilai dari Lesson yang sesuai
//                course.Lesson = lesson;

//                // Memperbarui entri di CourseClassRoom
//                course.CourseClassRooms.Clear(); // Hapus semua entri yang ada terlebih dahulu

//                foreach (var uniqueNumberOfClassRoom in request.CourseDto.UniqueNumberOfClassRooms)
//                {
//                    // Temukan ClassRoom yang sesuai berdasarkan UniqueNumberOfClassRoom
//                    var classRoom = await _context.ClassRooms.FirstOrDefaultAsync(x => x.UniqueNumberOfClassRoom == uniqueNumberOfClassRoom);
//                    if (classRoom == null)
//                    {
//                        return Result<CourseDto>.Failure($"ClassRoom with UniqueNumberOfClassRoom {uniqueNumberOfClassRoom} not found");
//                    }

//                    // Buat entri baru di CourseClassRoom
//                    course.CourseClassRooms.Add(new CourseClassRoom
//                    {
//                        CourseId = course.Id,
//                        ClassRoomId = classRoom.Id
//                    });
//                }

//                var result = await _context.SaveChangesAsync(cancellationToken) > 0;

//                if (!result)
//                {
//                    return Result<CourseDto>.Failure("Failed to edit Course");
//                }

//                // Buat instance CourseDto yang mewakili hasil edit
//                var editedCourseDto = _mapper.Map<CourseDto>(course);
//                editedCourseDto.UniqueNumberOfLesson = request.CourseDto.UniqueNumberOfLesson;
//                editedCourseDto.UniqueNumberOfClassRooms = request.CourseDto.UniqueNumberOfClassRooms;

//                return Result<CourseDto>.Success(editedCourseDto);
//            }
//        }
//    }
//}
