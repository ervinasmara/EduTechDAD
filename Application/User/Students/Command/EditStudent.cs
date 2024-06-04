using Application.Core;
using Application.User.DTOs.Edit;
using AutoMapper;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;
using static Application.User.Students.Command.EditStudent;

namespace Application.User.Students.Command;
public class EditStudent
{
    public class UpdateStudentCommand : IRequest<Result<EditStudentDto>>
    {
        public Guid StudentId { get; set; }
        public EditStudentDto StudentEditDto { get; set; }
    }

    public class UpdateStudentCommandHandler : IRequestHandler<UpdateStudentCommand, Result<EditStudentDto>>
    {
        private readonly DataContext _context;
        private readonly IMapper _mapper;

        public UpdateStudentCommandHandler(DataContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<Result<EditStudentDto>> Handle(UpdateStudentCommand request, CancellationToken cancellationToken)
        {
            /** Langkah 1: Mencari siswa berdasarkan ID **/
            var student = await _context.Students
                .Include(s => s.User) // Memuat relasi User
                .Include(s => s.ClassRoom) // Memuat relasi ClassRoom
                .FirstOrDefaultAsync(s => s.Id == request.StudentId);

            /** Langkah 2: Memeriksa apakah siswa ditemukan **/
            if (student == null)
            {
                return Result<EditStudentDto>.Failure("Siswa tidak ditemukan");
            }

            /** Langkah 3: Memetakan data dari DTO ke entitas siswa **/
            _mapper.Map(request.StudentEditDto, student);

            /** Langkah 4: Memeriksa apakah UniqueNumberOfClassRoom berubah **/
            if (student.ClassRoom?.UniqueNumberOfClassRoom != request.StudentEditDto.UniqueNumberOfClassRoom)
            {
                var classRoom = await _context.ClassRooms.FirstOrDefaultAsync(c => c.UniqueNumberOfClassRoom == request.StudentEditDto.UniqueNumberOfClassRoom);
                if (classRoom == null)
                {
                    return Result<EditStudentDto>.Failure("Nomor Unik kelas tidak ada");
                }

                // Tambahan pengecekan status ClassRoom
                if (classRoom.Status == 0) // 0 adalah status yang menunjukkan kelas tidak bisa digunakan
                {
                    return Result<EditStudentDto>.Failure("Kelas tidak bisa digunakan");
                }

                student.ClassRoom = classRoom;
            }

            /** Langkah 5: Memetakan entitas siswa yang telah diperbarui ke DTO **/
            var updatedStudentDto = _mapper.Map<EditStudentDto>(student);

            /** Langkah 6: Memperbarui nilai properti UniqueNumberOfClassRoom di DTO **/
            updatedStudentDto.UniqueNumberOfClassRoom = student.ClassRoom?.UniqueNumberOfClassRoom;

            /** Langkah 7: Menyimpan perubahan ke database **/
            _context.Students.Update(student);
            await _context.SaveChangesAsync();

            /** Langkah 8: Mengembalikan hasil dalam bentuk Success Result dengan data siswa yang diperbarui **/
            return Result<EditStudentDto>.Success(updatedStudentDto);
        }
    }
}

public class EditTeacherCommandValidator : AbstractValidator<EditStudentDto>
{
    public EditTeacherCommandValidator()
    {
        RuleFor(x => x.Address).NotEmpty().WithMessage("Alamat tidak boleh kosong");
        RuleFor(x => x.PhoneNumber)
            .NotEmpty().WithMessage("Nomor telepon tidak boleh kosong")
            .Matches("^[0-9]*$").WithMessage("Nomor telepon hanya boleh berisi angka")
            .Length(8, 13).WithMessage("Nomor telepon harus terdiri dari 8 hingga 13 digit")
            .Must(phone => phone.StartsWith("0")).WithMessage("Nomor telepon harus diawali dengan angka 0");
        RuleFor(x => x.UniqueNumberOfClassRoom).NotEmpty().WithMessage("Kelas tidak boleh kosong");
    }
}