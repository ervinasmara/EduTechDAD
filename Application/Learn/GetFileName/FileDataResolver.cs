using Application.Assignments;
using AutoMapper;
using Domain.Assignments;
using Domain.Learn.Courses;

namespace Application.Learn.GetFileName
{
    public class AssignmentFileDataResolver : IValueResolver<Assignment, DownloadFileDto, byte[]>
    {
        public byte[] Resolve(Assignment source, DownloadFileDto destination, byte[] destMember, ResolutionContext context)
        {
            if (!File.Exists(source.FilePath))
            {
                return null;
            }

            return File.ReadAllBytes(source.FilePath);
        }
    }

    public class CourseFileDataResolver : IValueResolver<Course, DownloadFileDto, byte[]>
    {
        public byte[] Resolve(Course source, DownloadFileDto destination, byte[] destMember, ResolutionContext context)
        {
            if (!File.Exists(source.FilePath))
            {
                return null;
            }

            return File.ReadAllBytes(source.FilePath);
        }
    }
}
