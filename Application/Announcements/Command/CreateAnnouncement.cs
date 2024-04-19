using MediatR;
using Persistence;
using FluentValidation;
using Application.Core;
using AutoMapper;
using Domain.Announcement;

namespace Application.Announcements.Command
{
    public class CreateAnnouncement
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
                // Membuat objek Announcement baru dengan menggunakan data yang diberikan dalam permintaan.
                var announcement = new Announcement
                {
                    Description = request.AnnouncementDto.Description,
                    Date = request.AnnouncementDto.Date
                };

                // Menambahkan objek Announcement baru ke konteks.
                _context.Announcements.Add(announcement);

                // Menyimpan perubahan ke basis data dan memeriksa apakah perubahan berhasil disimpan.
                var result = await _context.SaveChangesAsync(cancellationToken) > 0;

                // Jika gagal menyimpan perubahan, kembalikan pesan kesalahan.
                if (!result) return Result<AnnouncementDto>.Failure("Failed to create Announcement");

                // Membuat DTO dari objek Announcement yang telah dibuat.
                var announcementDto = _mapper.Map<AnnouncementDto>(announcement);

                // Mengembalikan hasil yang berhasil bersama dengan DTO Announcement yang telah dibuat.
                return Result<AnnouncementDto>.Success(announcementDto);
            }
        }
    }
}
