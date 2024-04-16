using MediatR;
using Persistence;
using FluentValidation;
using Application.Core;
using AutoMapper;
using Domain.Learn.Lessons;
using Microsoft.EntityFrameworkCore;

namespace Application.Learn.Lessons
{
    public class Create
    {
        public class Command : IRequest<Result<LessonCreateDto>>
        {
            public LessonCreateDto LessonCreateDto { get; set; }
        }

        public class CommandLesson : AbstractValidator<Command>
        {
            public CommandLesson()
            {
                RuleFor(x => x.LessonCreateDto).SetValidator(new LessonCreateValidator());
            }
        }

        public class Handler : IRequestHandler<Command, Result<LessonCreateDto>>
        {
            private readonly DataContext _context;
            private readonly IMapper _mapper;

            public Handler(DataContext context, IMapper mapper)
            {
                _context = context;
                _mapper = mapper;
            }

            public async Task<Result<LessonCreateDto>> Handle(Command request, CancellationToken cancellationToken)
            {
                try
                {
                    // Temukan Teacher berdasarkan NameTeacher yang diberikan
                    var teacher = await _context.Teachers.FirstOrDefaultAsync(t => t.NameTeacher == request.LessonCreateDto.NameTeacher);
                    if (teacher == null)
                        return Result<LessonCreateDto>.Failure($"Teacher with name {request.LessonCreateDto.NameTeacher} not found.");

                    var lastLesson = await _context.Lessons
                        .OrderByDescending(x => x.UniqueNumberOfLesson)
                        .FirstOrDefaultAsync(cancellationToken);

                    int newUniqueNumber = 1; // Nilai awal jika tidak ada lesson sebelumnya

                    if (lastLesson != null)
                    {
                        // Ambil nomor terakhir dan tambahkan 1
                        var lastUniqueNumber = int.Parse(lastLesson.UniqueNumberOfLesson);
                        newUniqueNumber = lastUniqueNumber + 1;
                    }

                    // Buat UniqueNumberOfLesson dengan format 2 digit
                    var uniqueNumber = newUniqueNumber.ToString("00");

                    var lesson = new Lesson
                    {
                        LessonName = request.LessonCreateDto.LessonName,
                        UniqueNumberOfLesson = uniqueNumber,
                        TeacherId = teacher.Id // Set TeacherId berdasarkan entitas Teacher yang ditemukan
                    };

                    _context.Lessons.Add(lesson);

                    var result = await _context.SaveChangesAsync(cancellationToken) > 0;

                    if (!result)
                        return Result<LessonCreateDto>.Failure("Failed to Create Lesson");

                    // Memuat kembali teacher untuk mendapatkan nama
                    teacher = await _context.Teachers.FindAsync(teacher.Id);

                    var lessonDto = _mapper.Map<LessonCreateDto>(lesson);
                    lessonDto.NameTeacher = teacher.NameTeacher; // Mengisi NameTeacher dalam DTO

                    return Result<LessonCreateDto>.Success(lessonDto);
                }
                catch (Exception ex)
                {
                    // Tangani pengecualian dan kembalikan pesan kesalahan yang sesuai
                    return Result<LessonCreateDto>.Failure($"Failed to create lesson: {ex.Message}");
                }
            }
        }
    }
}