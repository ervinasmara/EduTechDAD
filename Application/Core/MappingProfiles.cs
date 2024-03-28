using Application.Learn.Study;
using AutoMapper;
using Domain.Announcement;
using Domain.Class;
using Domain.Learn.Study;
using Domain.Present;
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
            /*CreateMap<Student, StudentGetAllDto>();
            CreateMap<Course, CourseDto>();
            CreateMap<CourseDto, Course>();
            CreateMap<Course, CourseGetAllDto>();
            CreateMap<CourseGetAllDto, Course>();
            CreateMap<CourseEditDto, Course>();
            CreateMap<Course, CourseEditDto>();*/

            // Konfigurasi pemetaan untuk Course
            CreateMap<Course, CourseDto>();
            CreateMap<CourseDto, Course>();
            CreateMap<Course, CourseGetAllDto>();
            CreateMap<CourseGetAllDto, Course>();

            // Konfigurasi pemetaan untuk CourseEditDto
            CreateMap<CourseEditDto, Course>()
                .ForMember(dest => dest.FileData, opt => opt.Ignore()); // Mengabaikan properti FileData saat memetakan dari CourseEditDto ke Course

            CreateMap<Course, CourseEditDto>()
                .ForMember(dest => dest.FileData, opt => opt.Ignore()); // Mengabaikan properti FileData saat memetakan dari Course ke CourseEditDto

        //CreateMap<EditCourse.Command, Course>()
        //    .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
        //    .ForMember(dest => dest.CourseName, opt => opt.MapFrom(src => src.CourseEditDto.CourseName))
        //    .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.CourseEditDto.Description));
    }
    }
}
