﻿using AutoMapper;
using Domain.Announcement;
using Domain.Class;
using Domain.Present;

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
        }
    }
}