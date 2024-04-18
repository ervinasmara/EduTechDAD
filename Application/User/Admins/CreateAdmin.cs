using Application.Core;
using MediatR;
using Persistence;
using Microsoft.EntityFrameworkCore;
using Application.User.DTOs;
using Domain.User;
using Microsoft.AspNetCore.Identity;
using Application.User.DTOs.Registration;
using FluentValidation;
using Application.User.Validation;

namespace Application.User.Admins
{
    public class CreateAdmin
    {
        public class RegisterAdminCommand : IRequest<Result<AdminGetDto>>
        {
            public RegisterAdminDto AdminDto { get; set; }
        }

        public class RegisterAdminCommandValidator : AbstractValidator<RegisterAdminCommand>
        {
            public RegisterAdminCommandValidator()
            {
                RuleFor(x => x.AdminDto).SetValidator(new RegisterAdminValidator());
            }
        }

        public class RegisterAdminCommandHandler : IRequestHandler<RegisterAdminCommand, Result<AdminGetDto>>
        {
            private readonly UserManager<AppUser> _userManager;
            private readonly DataContext _context;

            public RegisterAdminCommandHandler(UserManager<AppUser> userManager, DataContext context)
            {
                _userManager = userManager;
                _context = context;
            }

            public async Task<Result<AdminGetDto>> Handle(RegisterAdminCommand request, CancellationToken cancellationToken)
            {
                RegisterAdminDto adminDto = request.AdminDto;

                // Pemeriksaan username supaya berbeda dengan yang lain
                if (await _userManager.Users.AnyAsync(x => x.UserName == adminDto.Username))
                {
                    return Result<AdminGetDto>.Failure("Username already in use");
                }

                var user = new AppUser
                {
                    UserName = adminDto.Username,
                    Role = adminDto.Role,
                };

                var admin = new Admin
                {
                    NameAdmin = adminDto.NameAdmin,
                    AppUserId = user.Id
                };

                var result = await _userManager.CreateAsync(user, adminDto.Password);

                if (result.Succeeded)
                {
                    // Simpan admin ke dalam konteks database Anda
                    _context.Admins.Add(admin);
                    await _context.SaveChangesAsync();

                    // Gunakan metode CreateUserObjectAdmin untuk membuat objek AdminDto
                    var adminDtoResult = await CreateUserObjectAdminGet(user);
                    return Result<AdminGetDto>.Success(adminDtoResult); // Mengembalikan hasil sukses dengan DTO admin
                }

                return Result<AdminGetDto>.Failure(string.Join(",", result.Errors));
            }

            private async Task<AdminGetDto> CreateUserObjectAdminGet(AppUser user)
            {
                // Ambil data admin terkait dari database
                var admin = await _context.Admins.FirstOrDefaultAsync(g => g.AppUserId == user.Id);

                if (admin == null)
                {
                    // Handle jika data admin tidak ditemukan
                    throw new Exception("Admin data not found");
                }

                return new AdminGetDto
                {
                    Role = user.Role,
                    Username = user.UserName,
                    NameAdmin = admin.NameAdmin,
                };
            }
        }
    }
}