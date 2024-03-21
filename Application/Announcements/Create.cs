using MediatR;
using Persistence;
using FluentValidation;
using Application.Core;
using AutoMapper;
using Domain.Announcement;

namespace Application.Announcements
{
    public class Create
    {
        public class Command : IRequest<Result<AnnouncementDto>>
        {
            public AnnouncementDto AnnouncementDto { get; set; }
        }

        public class CommandValidator : AbstractValidator<Command>
        {
            public CommandValidator()
            {
                RuleFor(x => x.AnnouncementDto).SetValidator(new AnnouncementValidator());
            }
        }

        public class Handler : IRequestHandler<Command, Result<AnnouncementDto>>
        {
            private readonly DataContext _context;
            private readonly IMapper _mapper;

            public Handler(DataContext context, IMapper mapper)
            {
                _context = context;
                _mapper = mapper;
            }

            public async Task<Result<AnnouncementDto>> Handle(Command request, CancellationToken cancellationToken)
            {
                var announcement = new Announcement
                {
                    Description = request.AnnouncementDto.Description,
                    Date = request.AnnouncementDto.Date,
                };

                _context.Announcements.Add(announcement);

                var result = await _context.SaveChangesAsync(cancellationToken) > 0;

                if (!result) return Result<AnnouncementDto>.Failure("Gagal Untuk Membuat Announcement");

                // Buat DTO dari announcement yang telah dibuat
                var announcementDto = _mapper.Map<AnnouncementDto>(announcement);

                return Result<AnnouncementDto>.Success(announcementDto);
            }
        }
    }
}
