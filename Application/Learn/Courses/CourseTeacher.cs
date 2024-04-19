﻿using Application.Core;
using Application.Interface;
using Application.Learn.GetFileName;
using Application.Learn.Lessons;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Learn.Courses
{
    public class CourseTeacher
    {
        public class Query : IRequest<Result<object>>
        {
            // Tidak memerlukan properti karena hanya mengambil informasi dari token
        }

        public class QueryHandler : IRequestHandler<Query, Result<object>>
        {
            private readonly DataContext _context;
            private readonly IUserAccessor _userAccessor;

            public QueryHandler(DataContext context, IUserAccessor userAccessor)
            {
                _context = context;
                _userAccessor = userAccessor;
            }

            public async Task<Result<object>> Handle(Query request, CancellationToken cancellationToken)
            {
                var teacherId = _userAccessor.GetTeacherIdFromToken();

                if (teacherId == null)
                {
                    return Result<object>.Failure("Teacher ID not found in token");
                }

                var teacherLessons = await _context.TeacherLessons
                    .Where(tl => tl.TeacherId == Guid.Parse(teacherId))
                    .Select(tl => tl.Lesson)
                    .ToListAsync();

                var teacherCourses = await _context.TeacherCourses
                    .Include(tc => tc.Course)
                    .ThenInclude(c => c.CourseClassRooms)
                    .ThenInclude(ccr => ccr.ClassRoom)
                    .Where(tc => tc.TeacherId == Guid.Parse(teacherId))
                    .Select(tc => tc.Course)
                    .ToListAsync();

                var lessonDtos = teacherLessons.Select(lesson =>
                    new LessonGetTeacherDto
                    {
                        Id = lesson.Id,
                        LessonName = lesson.LessonName,
                        UniqueNumberOfLesson = lesson.UniqueNumberOfLesson,
                    }).ToList();

                var courseDtos = teacherCourses.Select(course =>
                {
                    var classRoomNames = course.CourseClassRooms.Select(ccr => ccr.ClassRoom.ClassName);

                    return new CourseTeacherGetDto
                    {
                        Id = course.Id,
                        CourseName = course.CourseName,
                        Description = course.Description,
                        FileName = course.FileData != null ? $"{course.CourseName}.{GetFileExtension.FileExtensionHelper(course.FileData)}" : "No File",
                        FileData = course.FileData,
                        LinkCourse = course.LinkCourse,
                        UniqueNumberOfLesson = course.Lesson != null ? course.Lesson.UniqueNumberOfLesson : "UnknownUniqueNumberOfLesson",
                        ClassNames = classRoomNames.ToList() // Menggunakan ClassName
                    };
                }).ToList();


                return Result<object>.Success(new
                {
                    teacherId = teacherId,
                    lessons = lessonDtos,
                    courses = courseDtos,
                });
            }
        }
    }
}