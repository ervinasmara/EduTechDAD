using Application.Core;
using Domain.Pengumuman;
using FluentValidation;
using MediatR;
using Persistence;

namespace Application.Pengumumans
{
    public class Create
    {
        public class Command : IRequest<Result<Unit>>
        {
            public Pengumuman Pengumuman { get; set; }
        }

        public class CommandValidator : AbstractValidator<Command>
        {
            public CommandValidator()
            {
                RuleFor(x => x.Pengumuman).SetValidator(new PengumumanValidator());
            }
        }

        public class Handler : IRequestHandler<Command, Result<Unit>>
        {
            private readonly DataContext _context;

            public Handler(DataContext context)
            {
                _context = context;
            }

            public async Task<Result<Unit>> Handle(Command request, CancellationToken cancellationToken)
            {
                _context.Pengumumans.Add(request.Pengumuman);

                var result = await _context.SaveChangesAsync() > 0;

                if (!result) return Result<Unit>.Failure("Gagal Untuk Create Pengumuman");

                return Result<Unit>.Success(Unit.Value);
            }
        }
    }
}