using Application.Core;
using MediatR;
using Persistence;
using Microsoft.EntityFrameworkCore;
using Domain.User;
using Microsoft.AspNetCore.Identity;
using Application.User.DTOs.Registration;
using FluentValidation;
using Application.User.Validation;
using Domain.Many_to_Many;
using Application.Interface;
using Application.Submission;
using Application.Interface.User;

namespace Application.User.Teachers.Command
{
    public class CreateTeacher
    {
        public class RegisterTeacherCommand : IRequest<Result<RegisterTeacherDto>>
        {
            public RegisterTeacherDto TeacherDto { get; set; }
        }

        public class RegisterTeacherCommandValidator : AbstractValidator<RegisterTeacherCommand>
        {
            public RegisterTeacherCommandValidator()
            {
                RuleFor(x => x.TeacherDto).SetValidator(new RegisterTeacherValidator());
            }
        }

        public class RegisterTeacherCommandHandler : IRequestHandler<RegisterTeacherCommand, Result<RegisterTeacherDto>>
        {
            private readonly UserManager<AppUser> _userManager;
            private readonly DataContext _context;
            private readonly IUserTeacher _userTeacher;

            public RegisterTeacherCommandHandler(UserManager<AppUser> userManager, DataContext context, IUserTeacher userTeacher)
            {
                _userManager = userManager;
                _context = context;
                _userTeacher = userTeacher;
            }

            public async Task<Result<RegisterTeacherDto>> Handle(RegisterTeacherCommand request, CancellationToken cancellationToken)
            {
                /** Langkah 1: Memanggil layanan untuk membuat pengajuan **/
                var createResult = await _userTeacher.CreateTeacherAsync(request.TeacherDto, cancellationToken);

                /** Langkah 2: Memeriksa apakah pembuatan pengajuan berhasil **/
                if (!createResult.IsSuccess)
                    return Result<RegisterTeacherDto>.Failure(createResult.Error);

                /** Langkah 3: Mengembalikan hasil yang berhasil **/
                return Result<RegisterTeacherDto>.Success(request.TeacherDto);
            }
        }
    }
}
