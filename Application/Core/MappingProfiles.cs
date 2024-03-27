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
            CreateMap<Student, StudentGetAllDto>();
            CreateMap<Course, CourseDto>();
            CreateMap<CourseDto, Course>();
            CreateMap<Course, CourseGetAllDto>();
            CreateMap<CourseGetAllDto, Course>();
        }
    }
}