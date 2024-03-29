using Application.Learn.Study;
using AutoMapper;
using Domain.Announcement;
using Domain.Class;
using Domain.Learn.Study;
using Domain.Present;
using Domain.Task;
using Domain.User;
using Domain.User.DTOs;

namespace Application.Core
{
    public class MappingProfiles : Profile
    {
        public MappingProfiles()
        {
            CreateMap<Announcement, AnnouncementDto>();
            CreateMap<AnnouncementDto, Announcement>();
            CreateMap<ClassRoom, ClassRoomDto>();
            CreateMap<ClassRoomDto, ClassRoom>();
            CreateMap<Attendance, AttendanceDto>();
            CreateMap<AttendanceDto, Attendance>();
            CreateMap<Assignment, AssignmentDto>();
            CreateMap<AssignmentDto, Assignment>();
            CreateMap<Assignment, AssignmentGetAllDto>();

            // Konfigurasi pemetaan untuk Course
            CreateMap<Course, CourseDto>();
            CreateMap<CourseDto, Course>();
            CreateMap<Course, CourseGetAllDto>();
            CreateMap<CourseGetAllDto, Course>();

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
