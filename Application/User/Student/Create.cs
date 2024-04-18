﻿using Application.Core;
using AutoMapper;
using Domain.Many_to_Many;
using Domain.Learn.Courses;
using FluentValidation;
using MediatR;
using Persistence;
using Microsoft.EntityFrameworkCore;
using Application.User.DTOs;
using Domain.User;
using Microsoft.AspNetCore.Identity;
using System.Net;

namespace Application.User.Student
{
    public class CreateSuperAdmin
    {
        public class RegisterSuperAdminCommand : IRequest<Result<SuperAdminGetDto>>
        {
            public RegisterSuperAdminDtoCoba SuperAdminDto { get; set; }
        }



        //public class CommandValidatorDto : AbstractValidator<Command>
        //{
        //    public CommandValidatorDto()
        //    {
        //        RuleFor(x => x.CourseDto).SetValidator(new CourseValidator());
        //    }
        //}

        public class RegisterSuperAdminCommandHandler : IRequestHandler<RegisterSuperAdminCommand, Result<SuperAdminGetDto>>
        {
            private readonly UserManager<AppUser> _userManager;
            private readonly DataContext _context;

            public RegisterSuperAdminCommandHandler(UserManager<AppUser> userManager, DataContext context)
            {
                _userManager = userManager;
                _context = context;
            }

            public async Task<Result<SuperAdminGetDto>> Handle(RegisterSuperAdminCommand request, CancellationToken cancellationToken)
            {
                RegisterSuperAdminDtoCoba superAdminDto = request.SuperAdminDto;

                // Validasi menggunakan FluentValidation
                //var validationResult = new RegisterSuperAdminCommandValidator().Validate(request);
                //if (!validationResult.IsValid)
                //{
                //    return Result<SuperAdminGetDto>.Failure("Validation failed");
                //}

                // Pemeriksaan username supaya berbeda dengan yang lain
                if (await _userManager.Users.AnyAsync(x => x.UserName == superAdminDto.Username))
                {
                    return Result<SuperAdminGetDto>.Failure("Username already exists.");
                }

                var user = new AppUser
                {
                    UserName = superAdminDto.Username,
                    Role = superAdminDto.Role,
                };

                var superAdmin = new SuperAdmin
                {
                    NameSuperAdmin = superAdminDto.NameSuperAdmin,
                    AppUserId = user.Id
                };

                var result = await _userManager.CreateAsync(user, superAdminDto.Password);

                if (result.Succeeded)
                {
                    // Simpan superAdmin ke dalam konteks database Anda
                    _context.SuperAdmins.Add(superAdmin);
                    await _context.SaveChangesAsync();

                    // Gunakan metode CreateUserObjectSuperAdmin untuk membuat objek SuperAdminDto
                    var superAdminDtoResult = await CreateUserObjectAdminGet(user);
                    return Result<SuperAdminGetDto>.Success(superAdminDtoResult); // Mengembalikan hasil sukses dengan DTO super admin
                }

                return Result<SuperAdminGetDto>.Failure(string.Join(",", result.Errors));
            }

            private async Task<SuperAdminGetDto> CreateUserObjectAdminGet(AppUser user)
            {
                // Ambil data admin terkait dari database
                var superAdmin = await _context.SuperAdmins.FirstOrDefaultAsync(g => g.AppUserId == user.Id);

                if (superAdmin == null)
                {
                    // Handle jika data super admin tidak ditemukan
                    throw new Exception("Super admin data not found");
                }

                return new SuperAdminGetDto
                {
                    Role = user.Role,
                    Username = user.UserName,
                    NameSuperAdmin = superAdmin.NameSuperAdmin,
                };
            }
        }
    }
}