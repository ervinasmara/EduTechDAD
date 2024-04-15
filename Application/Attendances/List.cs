using Application.Core;
using Application.Attendances.DTOs;
using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Attendances
{
    public class List
    {
        public class Query : IRequest<Result<List<AttendanceGetDto>>>
        {
            // Tidak ada parameter tambahan yang diperlukan
        }

        public class Handler : IRequestHandler<Query, Result<List<AttendanceGetDto>>>
        {
            private readonly DataContext _context;
            private readonly IMapper _mapper;

            public Handler(DataContext context, IMapper mapper)
            {
                _context = context;
                _mapper = mapper;
            }

            public async Task<Result<List<AttendanceGetDto>>> Handle(Query request, CancellationToken cancellationToken)
            {
                var attendances = await _context.Attendances
                    .ToListAsync(cancellationToken);

                var attendanceDtos = _mapper.Map<List<AttendanceGetDto>>(attendances);

                return Result<List<AttendanceGetDto>>.Success(attendanceDtos);
            }
        }
    }
}