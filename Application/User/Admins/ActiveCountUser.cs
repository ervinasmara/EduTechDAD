using Application.Core;
using Application.User.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.User.Admins;
public class ActiveCountUser
{
    public class ActiveCountQuery : IRequest<Result<ActiveCountDto>>
    {
    }

    // Query Handler
    public class ActiveCountQueryHandler : IRequestHandler<ActiveCountQuery, Result<ActiveCountDto>>
    {
        private readonly DataContext _context;

        public ActiveCountQueryHandler(DataContext context)
        {
            _context = context;
        }

        public async Task<Result<ActiveCountDto>> Handle(ActiveCountQuery request, CancellationToken cancellationToken)
        {
            var activeStudentCount = await _context.Students.CountAsync(s => s.Status == 1, cancellationToken);
            var activeTeacherCount = await _context.Teachers.CountAsync(t => t.Status == 1, cancellationToken);

            var activeCountDto = new ActiveCountDto
            {
                ActiveStudentCount = activeStudentCount,
                ActiveTeacherCount = activeTeacherCount
            };

            return Result<ActiveCountDto>.Success(activeCountDto);
        }
    }
}