using MediatR;
using Persistence;
using AutoMapper;
using FluentValidation;
using Application.Core;
using Domain.Pengumuman;

namespace Application.Pengumumans
{
    public class Edit
    {
        public class Command : IRequest<Result<PengumumanDto>>
        {
            public Guid Id { get; set; }
            public PengumumanDto PengumumanDto { get; set; }
        }

        public class CommandValidatorDto : AbstractValidator<Command>
        {
            public CommandValidatorDto()
            {
                RuleFor(x => x.PengumumanDto).SetValidator(new PengumumanValidator());
            }
        }

        public class Handler : IRequestHandler<Command, Result<PengumumanDto>>
        {
            private readonly DataContext _context;
            private readonly IMapper _mapper;

            public Handler(DataContext context, IMapper mapper)
            {
                _mapper = mapper;
                _context = context;
            }

            public async Task<Result<PengumumanDto>> Handle(Command request, CancellationToken cancellationToken)
            {
                var pengumuman = await _context.Pengumumans.FindAsync(request.Id);

                // Periksa apakah pengumuman ditemukan
                if (pengumuman == null)
                {
                    return Result<PengumumanDto>.Failure("Jurusan tidak ditemukan");
                }

                _mapper.Map(request.PengumumanDto, pengumuman);

                var result = await _context.SaveChangesAsync(cancellationToken) > 0;

                if (!result)
                {
                    return Result<PengumumanDto>.Failure("Gagal untuk mengedit Jurusan");
                }

                // Buat instance PengumumanDto yang mewakili hasil edit
                var editedPengumumanDto = _mapper.Map<PengumumanDto>(pengumuman);

                return Result<PengumumanDto>.Success(editedPengumumanDto);
            }
        }
    }
}
