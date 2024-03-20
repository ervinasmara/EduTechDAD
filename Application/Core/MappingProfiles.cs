using AutoMapper;
using Domain.Pengumuman;

namespace Application.Core
{
    public class MappingProfiles : Profile
    {
        public MappingProfiles()
        {
            CreateMap<Pengumuman, Pengumuman>();
        }
    }
}