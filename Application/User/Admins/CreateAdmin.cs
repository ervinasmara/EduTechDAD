using Application.Core;
using MediatR;
using Persistence;
using Microsoft.EntityFrameworkCore;
using Domain.User;
using Microsoft.AspNetCore.Identity;
using Application.User.DTOs.Registration;
using FluentValidation;
using AutoMapper;

namespace Application.User.Admins
{
    public class CreateAdmin
    {
        public class RegisterAdminCommand : IRequest<Result<RegisterAdminDto>>
        {
            public RegisterAdminDto AdminDto { get; set; }
        }

        public class RegisterAdminCommandValidator : AbstractValidator<RegisterAdminCommand>
        {
            public RegisterAdminCommandValidator()
            {
                RuleFor(x => x.AdminDto.NameAdmin).NotEmpty();
                RuleFor(x => x.AdminDto.Username).NotEmpty().Length(5, 20).WithMessage("Username length must be between 5 and 20 characters");
                RuleFor(x => x.AdminDto.Password)
                    .NotEmpty()
                    .Matches("(?=.*\\d)(?=.*[a-z])(?=.*[A-Z])(?!.*\\s).{8,16}")
                    .WithMessage("Password must be complex");
            }
        }

        public class RegisterAdminCommandHandler : IRequestHandler<RegisterAdminCommand, Result<RegisterAdminDto>>
        {
            private readonly UserManager<AppUser> _userManager;
            private readonly DataContext _context;
            private readonly IMapper _mapper;

            public RegisterAdminCommandHandler(UserManager<AppUser> userManager, DataContext context, IMapper mapper)
            {
                _userManager = userManager;
                _context = context;
                _mapper = mapper;
            }

            public async Task<Result<RegisterAdminDto>> Handle(RegisterAdminCommand request, CancellationToken cancellationToken)
            {
                RegisterAdminDto adminDto = request.AdminDto;

                /** Langkah 1: Pemeriksaan username untuk memastikan tidak ada yang sama dengan yang lain **/
                if (await _userManager.Users.AnyAsync(x => x.UserName == adminDto.Username))
                {
                    return Result<RegisterAdminDto>.Failure("Username already in use");
                }

                /** Langkah 2: Membuat instance AppUser baru untuk Admin **/
                var user = new AppUser
                {
                    UserName = adminDto.Username,
                    Role = 1, // Menetapkan peran (role) Admin
                };

                /** Langkah 3: Membuat pengguna baru di sistem **/
                var createUserResult = await _userManager.CreateAsync(user, adminDto.Password);
                if (!createUserResult.Succeeded)
                {
                    // Jika gagal membuat pengguna baru, kembalikan pesan kesalahan
                    return Result<RegisterAdminDto>.Failure(string.Join(",", createUserResult.Errors));
                }

                /** Langkah 4: Memetakan data Admin DTO ke entitas Admin **/
                var admin = _mapper.Map<Admin>(adminDto);
                admin.AppUserId = user.Id;

                /** Langkah 5: Menambahkan entitas Admin ke konteks database **/
                _context.Admins.Add(admin);

                /** Langkah 6: Simpan semua entitas yang terkait dalam satu panggilan SaveChangesAsync **/
                await _context.SaveChangesAsync(cancellationToken);

                /** Langkah 7: Memetakan kembali hasil ke DTO dan mengembalikannya **/
                var result = _mapper.Map<RegisterAdminDto>(admin);
                return Result<RegisterAdminDto>.Success(result);
            }
        }
    }
}