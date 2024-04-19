using MediatR;
using Persistence;
using AutoMapper;
using FluentValidation;
using Application.Core;

namespace Application.Announcements.Command
{
    public class EditAnnouncement
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
                // Mencari Announcement berdasarkan ID yang diberikan dalam permintaan.
                var announcement = await _context.Announcements.FindAsync(request.Id);

                // Periksa apakah Announcement ditemukan.
                if (announcement == null)
                {
                    return Result<AnnouncementDto>.Failure("Announcement not found");
                }

                // Memetakan properti AnnouncementDto yang diberikan dalam permintaan ke Announcement yang ditemukan.
                _mapper.Map(request.AnnouncementDto, announcement);

                // Menyimpan perubahan ke basis data dan memeriksa apakah perubahan berhasil disimpan.
                var result = await _context.SaveChangesAsync(cancellationToken) > 0;

                // Jika gagal menyimpan perubahan, kembalikan pesan kesalahan.
                if (!result)
                {
                    return Result<AnnouncementDto>.Failure("Failed to edit Announcement");
                }

                // Membuat instance AnnouncementDto yang mewakili hasil edit Announcement.
                var editedAnnouncementDto = _mapper.Map<AnnouncementDto>(announcement);

                // Mengembalikan hasil yang berhasil bersama dengan DTO Announcement yang telah diedit.
                return Result<AnnouncementDto>.Success(editedAnnouncementDto);
            }
        }
    }
}
