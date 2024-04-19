using Application.Core;
using Application.User.DTOs.Edit;
using Application.User.Validation;
using Domain.Class;
using Domain.Learn.Lessons;
using Domain.Many_to_Many;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;
using TeacherDomain = Domain.User.Teacher;

namespace Application.User.Teachers
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
                RuleFor(x => x.TeacherId).NotEmpty();
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
                    .Include(t => t.TeacherClassRooms)
                    .FirstOrDefaultAsync(t => t.Id == request.TeacherId);

                if (teacher == null)
                    return Result<EditTeacherDto>.Failure("Teacher not found");

                // Perbarui alamat dan nomor telepon
                teacher.Address = request.TeacherDto.Address;
                teacher.PhoneNumber = request.TeacherDto.PhoneNumber;

                // Dapatkan semua Pelajaran yang valid dari input guru
                var validLessons = await GetValidLessons(request.TeacherDto.LessonNames);

                if (validLessons.Count != request.TeacherDto.LessonNames.Count)
                {
                    var invalidLessonNames = request.TeacherDto.LessonNames.Except(validLessons.Select(l => l.LessonName));
                    return Result<EditTeacherDto>.Failure($"Lesson(s) '{string.Join(", ", invalidLessonNames)}' do not exist");
                }

                // Dapatkan semua Kelas yang valid yang terkait dengan Pelajaran yang dipilih dan juga termasuk dalam ClassNames
                var validClassRooms = await GetValidClassRooms(validLessons, request.TeacherDto.ClassNames);

                // Jika tidak ada ClassRooms yang valid, kembalikan error
                if (validClassRooms.Count != request.TeacherDto.ClassNames.Count)
                {
                    return Result<EditTeacherDto>.Failure($"One or more class(es) do not exist for the selected lesson(s)");
                }

                // Check if the selected lessons and classrooms are already assigned to other teachers
                var conflictingTeachers = await GetConflictingTeachers(validLessons, validClassRooms, teacher.Id);

                if (conflictingTeachers.Any())
                {
                    var conflictingTeacherNames = conflictingTeachers.Select(t => t.NameTeacher); // You can adjust this according to your DTO or result format

                    return Result<EditTeacherDto>.Failure($"The selected lessons and classrooms are already assigned to other teachers: {string.Join(", ", conflictingTeacherNames)}");
                }

                // Hapus relasi Pelajaran yang ada dan gantikan dengan yang baru
                teacher.TeacherLessons.Clear();
                foreach (var lesson in validLessons)
                {
                    teacher.TeacherLessons.Add(new TeacherLesson { TeacherId = teacher.Id, LessonId = lesson.Id });
                }

                // Hapus relasi Ruang Kelas yang ada dan gantikan dengan yang baru
                teacher.TeacherClassRooms.Clear();
                foreach (var classRoom in validClassRooms)
                {
                    teacher.TeacherClassRooms.Add(new TeacherClassRoom { TeacherId = teacher.Id, ClassRoomId = classRoom.Id });
                }

                var result = await _context.SaveChangesAsync() > 0;

                if (!result)
                    return Result<EditTeacherDto>.Failure("Failed to update teacher");

                var updatedTeacherDto = new EditTeacherDto
                {
                    Address = teacher.Address,
                    PhoneNumber = teacher.PhoneNumber,
                    LessonNames = request.TeacherDto.LessonNames ?? new List<string>(), // Inisialisasi dengan daftar kosong jika null
                    ClassNames = request.TeacherDto.ClassNames ?? new List<string>() // Inisialisasi dengan daftar kosong jika null
                };

                return Result<EditTeacherDto>.Success(updatedTeacherDto);
            }

            private async Task<List<TeacherDomain>> GetConflictingTeachers(List<Lesson> lessons, List<ClassRoom> classRooms, Guid teacherId)
            {
                var lessonIds = lessons.Select(l => l.Id).ToList();
                var classRoomIds = classRooms.Select(cr => cr.Id).ToList();

                // Ambil ID guru yang sudah terikat dengan pelajaran yang dipilih
                var conflictingTeacherIds = await _context.TeacherLessons
                    .Where(tl => lessonIds.Contains(tl.LessonId) && tl.TeacherId != teacherId)
                    .Select(tl => tl.TeacherId)
                    .ToListAsync();

                // Ambil ID guru yang sudah terikat dengan kelas yang dipilih
                var conflictingTeacherIdsFromClassRooms = await _context.TeacherClassRooms
                    .Where(tcr => classRoomIds.Contains(tcr.ClassRoomId) && tcr.TeacherId != teacherId)
                    .Select(tcr => tcr.TeacherId)
                    .ToListAsync();

                // Ambil ID guru yang terkait dengan keduanya
                var commonConflictingTeacherIds = conflictingTeacherIds.Intersect(conflictingTeacherIdsFromClassRooms).ToList();

                // Ambil entitas guru yang sesuai dengan ID yang ditemukan
                var conflictingTeachers = await _context.Teachers
                    .Where(t => commonConflictingTeacherIds.Contains(t.Id))
                    .ToListAsync();

                return conflictingTeachers;
            }


            private async Task<List<Lesson>> GetValidLessons(ICollection<string> lessonNames)
            {
                return await _context.Lessons
                    .Where(l => lessonNames.Contains(l.LessonName))
                    .ToListAsync();
            }

            private async Task<List<ClassRoom>> GetValidClassRooms(List<Lesson> lessons, ICollection<string> classNames)
            {
                var lessonIds = lessons.Select(l => l.Id).ToList();

                var classRooms = await _context.LessonClassRooms
                    .Include(lcr => lcr.ClassRoom)
                    .Where(lcr => lessonIds.Contains(lcr.LessonId) && classNames.Contains(lcr.ClassRoom.ClassName))
                    .Select(lcr => lcr.ClassRoom)
                    .ToListAsync();

                return classRooms;
            }
        }
    }
}