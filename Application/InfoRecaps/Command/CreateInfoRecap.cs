using MediatR;
using Persistence;
using FluentValidation;
using Application.Core;
using AutoMapper;
using Domain.InfoRecaps;

namespace Application.InfoRecaps
{
    public class CreateInfoRecap
    {
        public class Command : IRequest<Result<InfoRecapCreateDto>>
        {
            public InfoRecapCreateDto InfoRecapCreateDto { get; set; }
        }

        public class CommandValidator : AbstractValidator<Command>
        {
            public CommandValidator()
            {
                RuleFor(x => x.InfoRecapCreateDto).SetValidator(new InfoRecapValidator());
            }
        }

        public class Handler : IRequestHandler<Command, Result<InfoRecapCreateDto>>
        {
            private readonly DataContext _context;
            private readonly IMapper _mapper;

            public Handler(DataContext context, IMapper mapper)
            {
                _context = context;
                _mapper = mapper;
            }

            public async Task<Result<InfoRecapCreateDto>> Handle(Command request, CancellationToken cancellationToken)
            {
                // Membuat objek InfoRecap baru dengan menggunakan data yang diberikan dalam permintaan.
                var infoRecap = new InfoRecap
                {
                    Description = request.InfoRecapCreateDto.Description,
                    Status = request.InfoRecapCreateDto.Status,
                    LastStatusChangeDate = DateTime.UtcNow.AddHours(7) // Menetapkan waktu perubahan status terakhir
                };

                // Menambahkan objek InfoRecap baru ke konteks.
                _context.InfoRecaps.Add(infoRecap);

                // Menyimpan perubahan ke basis data dan memeriksa apakah perubahan berhasil disimpan.
                var result = await _context.SaveChangesAsync(cancellationToken) > 0;

                // Jika gagal menyimpan perubahan, kembalikan pesan kesalahan.
                if (!result) return Result<InfoRecapCreateDto>.Failure("Failed to create InfoRecap");

                // Membuat DTO dari objek InfoRecap yang telah dibuat.
                var infoRecapDto = _mapper.Map<InfoRecapCreateDto>(infoRecap);

                // Mengembalikan hasil yang berhasil bersama dengan DTO InfoRecap yang telah dibuat.
                return Result<InfoRecapCreateDto>.Success(infoRecapDto);
            }
        }
    }
}
