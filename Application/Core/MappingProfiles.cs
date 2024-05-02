using Application.ClassRooms;
using Application.Learn.Schedules;
using Application.Learn.Courses;
using Application.Learn.Lessons;
using Application.Submission;
using Application.Assignments;
using AutoMapper;
using Domain.Class;
using Domain.Learn.Schedules;
using Domain.Learn.Courses;
using Domain.Learn.Lessons;
using Domain.Attendances;
using Domain.Submission;
using Domain.Assignments;
using Application.User.DTOs;
using Domain.User;
using Application.Learn.GetFileName;
using Application.Attendances;
using Application.User.DTOs.Registration;

namespace Application.Core
{
    public class MappingProfiles : Profile
    {
        public MappingProfiles()
        {
            CreateMap<ClassRoom, ClassRoomCreateAndEditDto>();
            CreateMap<ClassRoom, ClassRoomGetDto>();
            CreateMap<ClassRoomCreateAndEditDto, ClassRoom>();


            CreateMap<Schedule, ScheduleCreateAndEditDto>();
            CreateMap<ScheduleCreateAndEditDto, Schedule>();

            CreateMap<Teacher, TeacherGetAllAndByIdDto>();

            /// ===================================== ASSIGNMENT ============================================== //
            /// ===================================== ASSIGNMENT ============================================== //
            /** Get Assignment All & By Id **/
            CreateMap<Assignment, AssignmentGetAllAndByIdDto>()
                .ForMember(dest => dest.NameTeacher, opt => opt.MapFrom(src =>
                    src.Course.Lesson.TeacherLessons.Select(tl => tl.Teacher.NameTeacher).FirstOrDefault()))
                .ForMember(dest => dest.LessonName, opt => opt.MapFrom(src => src.Course.Lesson.LessonName))
                .ForMember(dest => dest.ClassName, opt => opt.MapFrom(src => src.Course.Lesson.ClassRoom.ClassName))
                .ForMember(dest => dest.LongClassName, opt => opt.MapFrom(src => src.Course.Lesson.ClassRoom.LongClassName))
                .ForMember(dest => dest.AssignmentStatus, opt => opt.MapFrom(src => src.Status == 1 ? "IsActive" : "NonActive"))
                .ForMember(dest => dest.AssignmentFilePath, opt => opt.MapFrom(src => src.FilePath))
                .ForMember(dest => dest.AssignmentFileName, opt => opt.MapFrom(src =>
                    !string.IsNullOrEmpty(src.AssignmentName) && src.FilePath != null
                        ? $"{src.AssignmentName}{Path.GetExtension(src.FilePath)}"
                        : "No File"));

            /** Get Assignment By TeacherId **/
            CreateMap<Assignment, AssignmentGetByTeacherIdDto>()
                .ForMember(dest => dest.LessonName, opt => opt.MapFrom(src => src.Course.Lesson.LessonName))
                .ForMember(dest => dest.ClassName, opt => opt.MapFrom(src => src.Course.Lesson.ClassRoom.ClassName))
                .ForMember(dest => dest.AssignmentFilePath, opt => opt.MapFrom(src => src.FilePath))
                .ForMember(dest => dest.AssignmentFilePath, opt => opt.MapFrom(src => src.FilePath))
                .ForMember(dest => dest.AssignmentFileName, opt => opt.MapFrom(src =>
                    !string.IsNullOrEmpty(src.AssignmentName) && src.FilePath != null
                        ? $"{src.AssignmentName}{Path.GetExtension(src.FilePath)}"
                        : "No File"))
                .ForAllMembers(opts => opts.Condition((src, dest, member) => src.Status != 0));

            /** Get Assignment By ClassRoomId **/
            CreateMap<Assignment, AssignmentGetByClassRoomIdDto>()
                .ForMember(dest => dest.LessonName, opt => opt.MapFrom(src => src.Course.Lesson.LessonName))
                .ForMember(dest => dest.AssignmentFilePath, opt => opt.MapFrom(src => src.FilePath))
                .ForMember(dest => dest.AssignmentStatus, opt => opt.MapFrom(src => src.Status == 1 ? "IsActive" : "NonActive"))
                .ForMember(dest => dest.AssignmentFileName, opt => opt.MapFrom(src =>
                    !string.IsNullOrEmpty(src.AssignmentName) && src.FilePath != null
                        ? $"{src.AssignmentName}{Path.GetExtension(src.FilePath)}"
                        : "No File"));

            /** Create & Edit Assignment **/
            CreateMap<Assignment, AssignmentCreateAndEditDto>();
            CreateMap<AssignmentCreateAndEditDto, Assignment>()
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => 1)) // Set status menjadi 1 (Aktif)
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => DateTime.UtcNow.AddHours(7))) // Set waktu pembuatan
                .ForMember(dest => dest.FilePath, opt => opt.Ignore()); // FilePath akan dihandle secara terpisah

            /** Download Assignment **/
            CreateMap<Assignment, DownloadFileDto>()
                .ForMember(dest => dest.FileData, opt => opt.MapFrom<AssignmentFileDataResolver>()) // Menggunakan resolver kustom
                .ForMember(dest => dest.FileName, opt => opt.MapFrom(src => $"{src.AssignmentName}{Path.GetExtension(src.FilePath)}"))
                .ForMember(dest => dest.ContentType, opt => opt.MapFrom(src => FileHelper.GetContentType(Path.GetExtension(src.FilePath).TrimStart('.'))));

            /// ===================================== ASSIGNMENTSUBMISSION ============================================== //
            /// ===================================== ASSIGNMENTSUBMISSION ============================================== //
            /** Get Details AssignmentSubmission By AssignmentId And StudentId **/
            CreateMap<AssignmentSubmission, AssignmentSubmissionGetByAssignmentIdAndStudentId>()
                .ForMember(dest => dest.FileData, opt => opt.MapFrom(src => src.FilePath))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status == 1 ? "Sudah Dikerjakan" : "Sudah Dinilai"))
                .ForMember(dest => dest.SubmissionTimeStatus, opt => opt.MapFrom(src => src.SubmissionTime <= src.Assignment.AssignmentDeadline ? "Tepat Waktu" : "Pengumpulan Terlambat"));

            /** Get Grade For Teacher AssignmentSubmission By LessonId And AssignmentId **/
            CreateMap<AssignmentSubmission, AssignmentSubmissionListGradeDto>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.SubmissionTime, opt => opt.MapFrom(src => src.SubmissionTime))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status == 1 ? "Sudah Dikerjakan" : "Sudah Dinilai"))
                .ForMember(dest => dest.SubmissionTimeStatus, opt => opt.MapFrom(src => src.SubmissionTime <= src.Assignment.AssignmentDeadline ? "Tepat Waktu" : "Pengumpulan Terlambat"))
                .ForMember(dest => dest.Link, opt => opt.MapFrom(src => src.Link))
                .ForMember(dest => dest.Grade, opt => opt.MapFrom(src => src.Grade))
                .ForMember(dest => dest.Comment, opt => opt.MapFrom(src => src.Comment))
                .ForMember(dest => dest.AssignmentId, opt => opt.MapFrom(src => src.AssignmentId.ToString()))
                .ForMember(dest => dest.AssignmentName, opt => opt.MapFrom(src => src.Assignment.AssignmentName))
                .ForMember(dest => dest.StudentId, opt => opt.MapFrom(src => src.StudentId.ToString()))
                .ForMember(dest => dest.StudentName, opt => opt.MapFrom(src => src.Student.NameStudent))
                .ForMember(dest => dest.ClassName, opt => opt.MapFrom(src => src.Student.ClassRoom.ClassName))
                .ForMember(dest => dest.FileData, opt => opt.MapFrom(src => src.FilePath));

            /** Get Student Not Submitted For Teacher **/
            CreateMap<Student, NotSubmittedDto>()
                .ForMember(dest => dest.StudentId, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.StudentName, opt => opt.MapFrom(src => src.NameStudent))
                .ForMember(dest => dest.ClassName, opt => opt.MapFrom(src => src.ClassRoom.ClassName))
                .ForMember(dest => dest.LongClassName, opt => opt.MapFrom(src => src.ClassRoom.LongClassName));

            /** Create AssignmentSubmission By StudentId **/
            CreateMap<AssignmentSubmission, SubmissionCreateByStudentIdDto>();
            CreateMap<SubmissionCreateByStudentIdDto, AssignmentSubmission>()
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => 1)) // Set status menjadi 1 (Aktif)
                .ForMember(dest => dest.SubmissionTime, opt => opt.MapFrom(src => DateTime.UtcNow.AddHours(7))) // Set waktu pembuatan
                .ForMember(dest => dest.FilePath, opt => opt.Ignore()) // FilePath akan dihandle secara terpisah
                .ForMember(dest => dest.AssignmentId, opt => opt.Ignore()); // Karena AssignmentId akan di-set secara terpisah

            /** Edit AssignmentSubmission By TeacherId For Grades **/
            CreateMap<AssignmentSubmission, AssignmentSubmissionTeacherDto>();
            CreateMap<AssignmentSubmissionTeacherDto, AssignmentSubmission>()
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => 2));

            CreateMap<AssignmentSubmission, DownloadFileDto>()
                .ForMember(dest => dest.FileData, opt => opt.MapFrom<AssignmentSubmissionFileDataResolver>()) // Menggunakan resolver kustom
                .ForMember(dest => dest.FileName, opt => opt.MapFrom(src => $"{src.Student.NameStudent}_{src.Assignment.AssignmentName}{Path.GetExtension(src.FilePath)}"))
                .ForMember(dest => dest.ContentType, opt => opt.MapFrom(src => FileHelper.GetContentType(Path.GetExtension(src.FilePath).TrimStart('.'))));

            /// ===================================== ATTENDANCE ============================================== //
            /// ===================================== ATTENDANCE ============================================== //
            /** Get All Attendance **/
            CreateMap<Student, AttendanceGetDto>()
               .ForMember(dest => dest.StudentId, opt => opt.MapFrom(src => src.Id))
               .ForMember(dest => dest.NameStudent, opt => opt.MapFrom(src => src.NameStudent))
               .ForMember(dest => dest.UniqueNumberOfClassRoom, opt => opt.MapFrom(src => src.ClassRoom.UniqueNumberOfClassRoom))
               .ForMember(dest => dest.AttendanceStudent, opt => opt.MapFrom(src => src.Attendances.Where(a => a.StudentId == src.Id)));
            CreateMap<Attendance, AttendanceStudentDto>()
                .ForMember(dest => dest.AttendanceId, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.Date, opt => opt.MapFrom(src => src.Date))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status));

            /** Get Attendance By AttendanceId **/
            CreateMap<Attendance, AttendanceGetByIdDto>();

            /** Get Attendance By StudentId **/
            CreateMap<Attendance, AttendanceGetByStudentIdDto>();

            /** Create Attendance **/
            CreateMap<Attendance, AttendanceCreateDto>();
            CreateMap<AttendanceCreateDto, Attendance>();
            CreateMap<AttendanceStudentCreateDto, Attendance>();
            CreateMap<Attendance, AttendanceStudentCreateDto>();

            /** Edit Attendance **/
            CreateMap<Attendance, AttendanceEditDto>();
            CreateMap<AttendanceEditDto, Attendance>();

            /// ===================================== COURSE ============================================== //
            /// ===================================== COURSE ============================================== //
            /** Get Course All & By {...}Id **/
            CreateMap<Course, CourseGetDto>()
                .ForMember(dest => dest.ClassName, opt => opt.MapFrom(src => src.Lesson.ClassRoom.ClassName))
                .ForMember(dest => dest.LongClassName, opt => opt.MapFrom(src => src.Lesson.ClassRoom.LongClassName))
                .ForMember(dest => dest.LessonName, opt => opt.MapFrom(src => src.Lesson.LessonName))
                .ForMember(dest => dest.FileData, opt => opt.MapFrom(src => src.FilePath))
                .ForMember(dest => dest.NameTeacher, opt => opt.MapFrom(src =>
                    src.Lesson.TeacherLessons.Select(tl => tl.Teacher.NameTeacher).FirstOrDefault()))
                .ForMember(dest => dest.FileName, opt => opt.MapFrom(src =>
                    !string.IsNullOrEmpty(src.CourseName) && src.FilePath != null
                        ? $"{src.CourseName}{Path.GetExtension(src.FilePath)}"
                        : "No File"));

            /** Create & Edit Course **/
            CreateMap<Course, CourseCreateAndEditDto>();
            CreateMap<CourseCreateAndEditDto, Course>()
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => 1)) // Set status menjadi 1 (Aktif)
                .ForMember(dest => dest.FilePath, opt => opt.Ignore()) // FilePath akan dihandle secara terpisah
                .ForMember(dest => dest.LessonId, opt => opt.Ignore()) // Karena LessonId akan di-set secara terpisah
                .ForMember(dest => dest.Assignments, opt => opt.Ignore()) // Assignments tidak di-set dari DTO
                .AfterMap((src, dest, context) =>
                {
                    // Setelah mapping, kita perlu mengisi LessonId dari LessonName
                    var lesson = context.Items["Lesson"] as Lesson;
                    if (lesson != null)
                    {
                        dest.LessonId = lesson.Id;
                    }
                });

            /** Download Course **/
            CreateMap<Course, DownloadFileDto>()
                .ForMember(dest => dest.FileData, opt => opt.MapFrom<CourseFileDataResolver>()) // Menggunakan resolver kustom
                .ForMember(dest => dest.FileName, opt => opt.MapFrom(src => $"{src.CourseName}{Path.GetExtension(src.FilePath)}"))
                .ForMember(dest => dest.ContentType, opt => opt.MapFrom(src => FileHelper.GetContentType(Path.GetExtension(src.FilePath).TrimStart('.'))));

            /// ===================================== LEARN ============================================== //
            /// ===================================== LEARN ============================================== //
            /** Get Lesson All & By Id #1 **/
            //CreateMap<Lesson, LessonGetAllAndByIdDto>()
            //    .ForMember(dest => dest.ClassName, opt => opt.MapFrom(src => src.ClassRoom.ClassName))
            //    .ForMember(dest => dest.NameTeacher, opt => opt.MapFrom(src => src.TeacherLessons.Select(tl => tl.Teacher.NameTeacher).FirstOrDefault()));

            /** Get Lesson All & By Id #2 **/
            //CreateMap<Lesson, LessonGetAllAndByIdDto>()
            //    .ForMember(dest => dest.ClassName, opt => opt.MapFrom(src => src.ClassRoom.ClassName))
            //    .ForMember(dest => dest.NameTeacher, opt => opt.MapFrom(src =>
            //        src.TeacherLessons != null && src.TeacherLessons.Any()
            //            ? src.TeacherLessons.Select(tl => tl.Teacher.NameTeacher).FirstOrDefault()
            //            : "Belum Ada Guru"));

            /** Get Lesson All & By Id #3 **/
            CreateMap<Lesson, LessonGetAllAndByIdDto>()
                .ForMember(dest => dest.ClassName, opt => opt.MapFrom(src => src.ClassRoom.ClassName))
                .ForMember(dest => dest.NameTeacher, opt => opt.MapFrom(src =>
                    src.TeacherLessons.Select(tl => tl.Teacher.NameTeacher).FirstOrDefault() ?? "Belum Ada Guru"))
                .ForMember(dest => dest.LessonStatus, opt => opt.MapFrom(src => src.Status == 1 ? "Aktif" : "Tidak Aktif"));

            /** Get Lesson By ClassRoomId **/
            CreateMap<Lesson, LessonGetByTeacherIdOrClassRoomIdDto>();

            /** Create Lesson **/
            CreateMap<Lesson, LessonCreateAndEditDto>();
            CreateMap<LessonCreateAndEditDto, Lesson>()
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => 1)) // Set status menjadi 1 (Aktif)
                .ForMember(dest => dest.UniqueNumberOfLesson, opt => opt.Ignore()) // UniqueNumber akan dihandle secara terpisah
                .AfterMap((src, dest, context) => {
                    // Temukan kelas berdasarkan nama yang diberikan
                    var classroom = context.Items["ClassRoom"] as ClassRoom;
                    if (classroom != null)
                    {
                        dest.ClassRoomId = classroom.Id; // Simpan ID kelas
                    }
                });

            /// ===================================== SCHEDULE ============================================== //
            /// ===================================== SCHEDULE ============================================== //
            /** Get All Schedule **/
            CreateMap<Schedule, ScheduleGetDto>()
                .ForMember(dest => dest.LessonName, opt => opt.MapFrom(src => src.Lesson.LessonName))
                .ForMember(dest => dest.ClassName, opt => opt.MapFrom(src => src.Lesson.ClassRoom.ClassName))
                .ForMember(dest => dest.NameTeacher, opt => opt.MapFrom(src =>
                    src.Lesson.TeacherLessons.Select(tl => tl.Teacher.NameTeacher).FirstOrDefault() ?? "Belum Ada Guru"));

            /** Create And Edit Schedule **/
            CreateMap<Schedule, ScheduleCreateAndEditDto>()
                .ForMember(dest => dest.LessonName, opt => opt.MapFrom(src => src.Lesson.LessonName));

            /// ===================================== STUDENT ============================================== //
            /// ===================================== STUDENT ============================================== //
            /** Get All Student **/
            CreateMap<Student, StudentGetAllDto>()
                .ForMember(dest => dest.Username, opt => opt.MapFrom(src => src.User.UserName))
                .ForMember(dest => dest.Role, opt => opt.MapFrom(src => src.User.Role))
                .ForMember(dest => dest.ClassName, opt => opt.MapFrom(src => src.ClassRoom.ClassName))
                .ForMember(dest => dest.UniqueNumberOfClassRoom, opt => opt.MapFrom(src => src.ClassRoom.UniqueNumberOfClassRoom))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status == 1 ? "Aktif" : "Tidak Aktif"));

            /** Get Count From Student And Teacher **/
            CreateMap<Student, ActiveCountDto>()
                .ForMember(dest => dest.ActiveStudentCount, opt => opt.MapFrom(src => src.Status == 1 ? 1 : 0))
                .ForMember(dest => dest.ActiveTeacherCount, opt => opt.Ignore());
            CreateMap<Teacher, ActiveCountDto>()
                .ForMember(dest => dest.ActiveTeacherCount, opt => opt.MapFrom(src => src.Status == 1 ? 1 : 0))
                .ForMember(dest => dest.ActiveStudentCount, opt => opt.Ignore());


            /** Create Teacher **/
            CreateMap<Teacher, RegisterTeacherDto>();
            CreateMap<RegisterTeacherDto, Teacher>();
        }
    }
}
