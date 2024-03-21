using MediatR;
using Persistence;
using FluentValidation;
using Application.Core;
using AutoMapper;
using Domain.Pengumuman;

namespace Application.Pengumumans
{
    public class Create
    {
        public class Command : IRequest<Result<PengumumanDto>>
        {
            public PengumumanDto PengumumanDto { get; set; }
        }

        public class CommandValidator : AbstractValidator<Command>
        {
            public CommandValidator()
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
                _context = context;
                _mapper = mapper;
            }

            public async Task<Result<PengumumanDto>> Handle(Command request, CancellationToken cancellationToken)
            {
                var pengumuman = new Pengumuman
                {
                    Deskripsi = request.PengumumanDto.Deskripsi,
                    Tanggal = request.PengumumanDto.Tanggal,
                };

                _context.Pengumumans.Add(pengumuman);

                var result = await _context.SaveChangesAsync(cancellationToken) > 0;

                if (!result) return Result<PengumumanDto>.Failure("Gagal Untuk Membuat Pengumuman");

                // Buat DTO dari pengumuman yang telah dibuat
                var pengumumanDto = _mapper.Map<PengumumanDto>(pengumuman);

                return Result<PengumumanDto>.Success(pengumumanDto);
            }
        }
    }
}
