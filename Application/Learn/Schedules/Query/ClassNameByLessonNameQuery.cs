using Application.Core;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Learn.Schedules.Query
{
    public class ClassNameByLessonName
    {
        public class ClassNameByLessonNameQuery : IRequest<Result<List<string>>>
        {
            public string LessonName { get; }

            public ClassNameByLessonNameQuery(string lessonName)
            {
                LessonName = lessonName;
            }
        }

        public class ClassNameByLessonNameQueryHandler : IRequestHandler<ClassNameByLessonNameQuery, Result<List<string>>>
        {
            private readonly DataContext _context;

            public ClassNameByLessonNameQueryHandler(DataContext context)
            {
                _context = context;
            }

            public async Task<Result<List<string>>> Handle(ClassNameByLessonNameQuery request, CancellationToken cancellationToken)
            {
                var classNames = await _context.Lessons
                    .Where(l => l.LessonName == request.LessonName)
                    .SelectMany(l => l.LessonClassRooms.Select(lc => lc.ClassRoom.ClassName))
                    .Distinct()
                    .ToListAsync(cancellationToken);

                if (classNames == null || !classNames.Any())
                {
                    return Result<List<string>>.Failure("No ClassNames associated with the provided LessonName.");
                }

                return Result<List<string>>.Success(classNames);
            }
        }
    }
}