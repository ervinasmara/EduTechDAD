using MediatR;
using Persistence;
using FluentValidation;
using Application.Core;
using AutoMapper;
using Domain.InfoRecaps;

namespace Application.InfoRecaps
{
    public class Create
    {
        public class Command : IRequest<Result<InfoRecapDto>>
        {
            public InfoRecapDto InfoRecapDto { get; set; }
        }

        public class CommandValidator : AbstractValidator<Command>
        {
            public CommandValidator()
            {
                RuleFor(x => x.InfoRecapDto).SetValidator(new InfoRecapValidator());
            }
        }

        public class Handler : IRequestHandler<Command, Result<InfoRecapDto>>
        {
            private readonly DataContext _context;
            private readonly IMapper _mapper;

            public Handler(DataContext context, IMapper mapper)
            {
                _context = context;
                _mapper = mapper;
            }

            public async Task<Result<InfoRecapDto>> Handle(Command request, CancellationToken cancellationToken)
            {
                var infoRecap = new InfoRecap
                {
                    Description = request.InfoRecapDto.Description,
                    Status = request.InfoRecapDto.Status,
                    LastStatusChangeDate = DateTime.UtcNow.AddHours(7)
                };

                _context.InfoRecaps.Add(infoRecap);

                var result = await _context.SaveChangesAsync(cancellationToken) > 0;

                if (!result) return Result<InfoRecapDto>.Failure("Failed to create InfoRecap");

                // Membuat DTO dari infoRecap yang telah dibuat
                var infoRecapDto = _mapper.Map<InfoRecapDto>(infoRecap);

                return Result<InfoRecapDto>.Success(infoRecapDto);
            }
        }
    }
}
