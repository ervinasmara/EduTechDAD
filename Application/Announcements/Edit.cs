using MediatR;
using Persistence;
using AutoMapper;
using FluentValidation;
using Application.Core;
using Domain.Announcement;

namespace Application.Announcements
{
    public class Edit
    {
        public class Command : IRequest<Result<AnnouncementDto>>
        {
            public Guid Id { get; set; }
            public AnnouncementDto AnnouncementDto { get; set; }
        }

        public class CommandValidatorDto : AbstractValidator<Command>
        {
            public CommandValidatorDto()
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
                _mapper = mapper;
                _context = context;
            }

            public async Task<Result<AnnouncementDto>> Handle(Command request, CancellationToken cancellationToken)
            {
                var announcement = await _context.Announcements.FindAsync(request.Id);

                // Periksa apakah announcement ditemukan
                if (announcement == null)
                {
                    return Result<AnnouncementDto>.Failure("Announcement not found");
                }

                _mapper.Map(request.AnnouncementDto, announcement);

                var result = await _context.SaveChangesAsync(cancellationToken) > 0;

                if (!result)
                {
                    return Result<AnnouncementDto>.Failure("Failed to edit Announcement");
                }

                // Buat instance AnnouncementDto yang mewakili hasil edit
                var editedAnnouncementDto = _mapper.Map<AnnouncementDto>(announcement);

                return Result<AnnouncementDto>.Success(editedAnnouncementDto);
            }
        }
    }
}
