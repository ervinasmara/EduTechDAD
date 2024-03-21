using Application.Core;
using AutoMapper;
using Domain.Pengumuman;
using FluentValidation;
using MediatR;
using Persistence;

namespace Application.Pengumumans
{
    public class Edit
    {
        public class Command : IRequest<Result<Unit>>
        {
            public Pengumuman Pengumuman { get; set; }
        }

        public class CommandValidator : AbstractValidator<Command>
        {
            public CommandValidator()
            {
                // Di sini kita akan membuat aturan untuk apa yang akan kita validasi
                RuleFor(x => x.Pengumuman).SetValidator(new PengumumanValidator());
            }
        }

        public class Handler : IRequestHandler<Command, Result<Unit>>
        {
            private readonly IMapper _mapper;
            private readonly DataContext _context;

            public Handler(DataContext context, IMapper mapper)
            {
                _mapper = mapper;
                _context = context;
            }

            public async Task<Result<Unit>> Handle(Command request, CancellationToken cancellationToken)
            {
                var pengumuman = await _context.Pengumumans.FindAsync(request.Pengumuman.Id);

                // Kita dapat menguji untuk melihat apakah pengumuman tersebut nol (null/kosong)
                if (pengumuman == null) return null;

                _mapper.Map(request.Pengumuman, pengumuman);

                var result = await _context.SaveChangesAsync() > 0;

                if (!result) return Result<Unit>.Failure("Gagal untuk edit Pengumuman");

                return Result<Unit>.Success(Unit.Value);
            }
        }
    }
}