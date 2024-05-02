using Application.Core;
using MediatR;
using Persistence;
using Microsoft.EntityFrameworkCore;
using Domain.User;
using Microsoft.AspNetCore.Identity;
using Application.User.DTOs.Registration;
using FluentValidation;
using AutoMapper;

namespace Application.User.Superadmin
{
    public class CreateSuperAdmin
    {
        public class RegisterSuperAdminCommand : IRequest<Result<RegisterSuperAdminDto>>
        {
            public RegisterSuperAdminDto SuperAdminDto { get; set; }
        }

        public class RegisterSuperAdminCommandValidator : AbstractValidator<RegisterSuperAdminCommand>
        {
            public RegisterSuperAdminCommandValidator()
            {
                RuleFor(x => x.SuperAdminDto.NameSuperAdmin).NotEmpty();
                RuleFor(x => x.SuperAdminDto.Username).NotEmpty().Length(5, 20).WithMessage("Username length must be between 5 and 20 characters");
                RuleFor(x => x.SuperAdminDto.Password)
                    .NotEmpty()
                    .Matches("(?=.*\\d)(?=.*[a-z])(?=.*[A-Z])(?!.*\\s).{8,16}")
                    .WithMessage("Password must be complex");
            }
        }

        public class RegisterSuperAdminCommandHandler : IRequestHandler<RegisterSuperAdminCommand, Result<RegisterSuperAdminDto>>
        {
            private readonly UserManager<AppUser> _userManager;
            private readonly DataContext _context;
            private readonly IMapper _mapper;

            public RegisterSuperAdminCommandHandler(UserManager<AppUser> userManager, DataContext context, IMapper mapper)
            {
                _userManager = userManager;
                _context = context;
                _mapper = mapper;
            }

            public async Task<Result<RegisterSuperAdminDto>> Handle(RegisterSuperAdminCommand request, CancellationToken cancellationToken)
            {
                var superAdminDto = request.SuperAdminDto;

                /** Langkah 1: Pemeriksaan username untuk memastikan tidak ada yang sama dengan yang lain **/
                if (await _userManager.Users.AnyAsync(x => x.UserName == superAdminDto.Username))
                {
                    return Result<RegisterSuperAdminDto>.Failure("Username already exists.");
                }

                /** Langkah 2: Membuat instance AppUser baru untuk Super Admin **/
                var user = new AppUser
                {
                    UserName = superAdminDto.Username,
                    Role = 4, // Menetapkan peran (role) Super Admin
                };

                /** Langkah 3: Membuat pengguna baru di sistem **/
                var createUserResult = await _userManager.CreateAsync(user, superAdminDto.Password);
                if (!createUserResult.Succeeded)
                {
                    // Jika gagal membuat pengguna baru, kembalikan pesan kesalahan
                    return Result<RegisterSuperAdminDto>.Failure(string.Join(",", createUserResult.Errors));
                }

                /** Langkah 4: Memetakan data Super Admin DTO ke entitas SuperAdmin **/
                var superAdmin = _mapper.Map<SuperAdmin>(superAdminDto);
                superAdmin.AppUserId = user.Id;

                /** Langkah 5: Menambahkan entitas Super Admin ke konteks database **/
                _context.SuperAdmins.Add(superAdmin);

                /** Langkah 6: Simpan semua entitas yang terkait dalam satu panggilan SaveChangesAsync **/
                await _context.SaveChangesAsync(cancellationToken);

                /** Langkah 7: Memetakan kembali hasil ke DTO dan mengembalikannya **/
                var result = _mapper.Map<RegisterSuperAdminDto>(superAdmin);
                return Result<RegisterSuperAdminDto>.Success(result);
            }
        }
    }
}