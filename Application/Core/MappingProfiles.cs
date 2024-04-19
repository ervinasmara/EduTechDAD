using Application.Announcements;
using Application.ClassRooms;
using Application.InfoRecaps;
using Application.Learn.Schedules;
using Application.Learn.Courses;
using Application.Learn.Lessons;
using Application.Attendances.DTOs;
using Application.Submission;
using Application.Tasks;
using AutoMapper;
using Domain.Announcement;
using Domain.Class;
using Domain.InfoRecaps;
using Domain.Learn.Schedules;
using Domain.Learn.Courses;
using Domain.Learn.Lessons;
using Domain.Attendances;
using Domain.Submission;
using Domain.Assignments;
using Domain.User;
using Application.User.DTOs;

namespace Application.Core
{
    public class MappingProfiles : Profile
    {
        public MappingProfiles()
        {
            CreateMap<Announcement, AnnouncementDto>();
            CreateMap<AnnouncementDto, Announcement>();
            CreateMap<ClassRoom, ClassRoomDto>();
            CreateMap<ClassRoom, ClassRoomGetDto>();
            CreateMap<ClassRoomDto, ClassRoom>();
            CreateMap<Attendance, AttendanceDto>();
            CreateMap<AttendanceDto, Attendance>();
            CreateMap<Attendance, AttendanceGetDto>();
            CreateMap<Attendance, AttendanceGetAllDto>();
            CreateMap<Attendance, AttendanceEditDto>();
            CreateMap<AttendanceEditDto, Attendance>();
            CreateMap<Assignment, AssignmentDto>();
            CreateMap<AssignmentDto, Assignment>();
            CreateMap<Assignment, AssignmentGetDto>();
            CreateMap<Lesson, LessonDto>();
            CreateMap<LessonDto, Lesson>();
            CreateMap<Lesson, LessonCreateDto>();
            CreateMap<Lesson, LessonGetAllDto>();
            CreateMap<LessonCreateDto, Lesson>();
            CreateMap<Schedule, ScheduleDto>();
            CreateMap<ScheduleDto, Schedule>();
            CreateMap<Schedule, ScheduleGetDto>();
            CreateMap<InfoRecap, InfoRecapCreateDto>();
            CreateMap<Teacher, TeacherGetAllDto>();


            CreateMap<AssignmentSubmission, AssignmentSubmissionStatusDto>();
            CreateMap<AssignmentSubmission, AssignmentSubmissionStudentDto>();
            CreateMap<AssignmentSubmission, AssignmentSubmissionTeacherDto>();
            CreateMap<AssignmentSubmission, AssignmentSubmissionGetDto>();
            CreateMap<AssignmentSubmission, AssignmentSubmissionGetByIdCRandA>();
            //CreateMap<AssignmentSubmissionGetByIdCRandA, AssignmentSubmission>();

            CreateMap<AssignmentSubmission, AssignmentSubmissionStudentDto>()
                .ForMember(dest => dest.FileData, opt => opt.Ignore());

            // Konfigurasi pemetaan untuk Course
            CreateMap<Course, CourseDto>();
            CreateMap<CourseDto, Course>();
            CreateMap<Course, CourseGetDto>();
            CreateMap<CourseGetDto, Course>();

            // Mapping untuk Course dan CourseDto
            CreateMap<Course, CourseDto>()
                .ForMember(dest => dest.FileData, opt => opt.Ignore()); // Mengabaikan properti FileData saat memetakan dari Course ke CourseDto

            CreateMap<CourseDto, Course>()
                .ForMember(dest => dest.FileData, opt => opt.Ignore()); // Mengabaikan properti FileData saat memetakan dari CourseDto ke Course

        // Konfigurasi pemetaan untuk CourseEditDto
        CreateMap<CourseEditDto, Course>()
            .ForMember(dest => dest.FileData, opt => opt.Ignore()); // Mengabaikan properti FileData saat memetakan dari CourseEditDto ke Course

        CreateMap<Course, CourseEditDto>()
            .ForMember(dest => dest.FileData, opt => opt.Ignore()); // Mengabaikan properti FileData saat memetakan dari Course ke CourseEditDto
    }
    }
}
