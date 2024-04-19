using MediatR;
using Persistence;
using AutoMapper;
using FluentValidation;
using Application.Core;
using Domain.InfoRecaps;

namespace Application.InfoRecaps
{
    public class EditInfoRecap
    {
        public class Command : IRequest<Result<InfoRecap>>
        {
            public Guid Id { get; set; }
            public InfoRecap InfoRecap { get; set; }
        }

        public class Handler : IRequestHandler<Command, Result<InfoRecap>>
        {
            private readonly DataContext _context;
            private readonly IMapper _mapper;

            public Handler(DataContext context, IMapper mapper)
            {
                _mapper = mapper;
                _context = context;
            }

            public async Task<Result<InfoRecap>> Handle(Command request, CancellationToken cancellationToken)
            {
                // Menggunakan FindAsync untuk mencari InfoRecap berdasarkan ID yang diberikan dalam permintaan.
                var infoRecap = await _context.InfoRecaps.FindAsync(request.Id);

                // Periksa apakah infoRecap ditemukan
                if (infoRecap == null)
                {
                    return Result<InfoRecap>.Failure("InfoRecap not found");
                }

                // Update properti Status menjadi 2
                infoRecap.Status = 2;

                var result = await _context.SaveChangesAsync(cancellationToken) > 0;

                if (!result)
                {
                    return Result<InfoRecap>.Failure("Failed to edit InfoRecap");
                }

                // Membuat instance InfoRecap yang mewakili hasil edit
                var editedInfoRecap = _mapper.Map<InfoRecap>(infoRecap);

                return Result<InfoRecap>.Success(editedInfoRecap);
            }
        }
    }
}
