using Application.Core;
using MediatR;
using Persistence;
using Domain.User;
using Microsoft.AspNetCore.Identity;
using Application.User.DTOs.Registration;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using FluentValidation;
using NPOI.XSSF.UserModel;

namespace Application.User.Students.Command;
    public class CreateStudentWithExcel
    {
        public class UploadStudentExcelCommand : IRequest<Result<List<RegisterStudentExcelDto>>>
        {
            public IFormFile File { get; set; }
        }

        public class RegisterStudentCommandValidator : AbstractValidator<RegisterStudentExcelDto>
        {
            public RegisterStudentCommandValidator()
            {
                RuleFor(x => x.NameStudent).NotEmpty().WithMessage("Nama tidak boleh kosong");
                RuleFor(x => x.BirthDate).NotEmpty().WithMessage("Tanggal lahir tidak boleh kosong");
                RuleFor(x => x.BirthPlace).NotEmpty().WithMessage("Tempat lahir tidak boleh kosong");
                RuleFor(x => x.Address).NotEmpty().WithMessage("Alamat tidak boleh kosong");
                RuleFor(x => x.PhoneNumber)
                    .NotEmpty().WithMessage("Nomor telepon tidak boleh kosong")
                    .Matches("^[0-9\\-+]*$").WithMessage("Nomor telepon hanya boleh berisi angka, tanda minus (-), atau tanda plus (+).")
                    .Length(8, 13).WithMessage("Nomor telepon harus terdiri dari 8 hingga 13 digit");
                RuleFor(x => x.Nis).NotEmpty().WithMessage("NIS tidak boleh kosong");
                RuleFor(x => x.ParentName).NotEmpty().WithMessage("Nama orang tua tidak boleh kosong");
                RuleFor(x => x.Gender)
                    .NotEmpty().WithMessage("Jenis kelamin tidak boleh kosong")
                    .Must(gender => gender >= 1 && gender <= 2)
                    .WithMessage("Nilai Jenis kelamin tidak valid. Gunakan 1 untuk Laki-laki, 2 untuk Perempuan");
                RuleFor(x => x.Username)
                .NotEmpty().WithMessage("Nama pengguna tidak boleh kosong")
                .Length(5, 20).WithMessage("Panjang nama pengguna harus antara 5 hingga 20 karakter")
                .Must(x => !x.Contains(" ")).WithMessage("Nama pengguna tidak boleh mengandung spasi");
                RuleFor(x => x.Password)
                    .NotEmpty().WithMessage("Kata sandi tidak boleh kosong")
                    .Matches("(?=.*\\d)(?=.*[a-z])(?=.*[A-Z])(?!.*\\s).{8,16}")
                    .WithMessage("Kata sandi harus memenuhi kriteria berikut: minimal 8 karakter, maksimal 16 karakter, setidaknya satu huruf kecil, satu huruf besar, dan satu angka");
                RuleFor(x => x.Role).NotEmpty().Equal(3).WithMessage("Role harus 3");
                RuleFor(x => x.UniqueNumberOfClassRoom).NotEmpty().WithMessage("Nomor unik ruang kelas tidak boleh kosong");
        }
        }

        public class UploadStudentExcelCommandHandler : IRequestHandler<UploadStudentExcelCommand, Result<List<RegisterStudentExcelDto>>>
        {
            private readonly UserManager<AppUser> _userManager;
            private readonly DataContext _context;
            private readonly IMapper _mapper;

            public UploadStudentExcelCommandHandler(UserManager<AppUser> userManager, DataContext context, IMapper mapper)
            {
                _userManager = userManager;
                _context = context;
                _mapper = mapper;
            }

            public async Task<Result<List<RegisterStudentExcelDto>>> Handle(UploadStudentExcelCommand request, CancellationToken cancellationToken)
            {
                /** Langkah 1: Memeriksa validitas berkas **/
                if (request.File == null || request.File.Length <= 0)
                {
                    return Result<List<RegisterStudentExcelDto>>.Failure("File tidak valid");
                }

                var studentsToSave = new List<RegisterStudentExcelDto>(); // Menyimpan siswa yang lolos validasi
                var errors = new List<string>();

                using (var stream = new MemoryStream())
                {
                    await request.File.CopyToAsync(stream);
                    stream.Position = 0;

                    using (var workbook = new XSSFWorkbook(stream))
                    {
                        var worksheet = workbook.GetSheetAt(0); /** Langkah 2: Mendapatkan lembar kerja (sheet) pertama dari buku kerja (workbook) **/
                        var rowCount = worksheet.LastRowNum; // Mendapatkan jumlah baris terakhir dalam lembar kerja

                        var nisList = new HashSet<string>(); // Menggunakan HashSet untuk menyimpan NIS yang sudah diverifikasi
                        var existingNisList = await _context.Students.Select(s => s.Nis).ToListAsync(); // Mendapatkan daftar NIS yang sudah ada di database

                        /** Langkah 3: Iterasi melalui setiap baris dalam lembar kerja **/
                        for (int row = 1; row <= rowCount; row++) // Mulai dari baris kedua, karena baris pertama mungkin berisi header
                        {
                            var rowValues = new List<string>();

                            /** Langkah 4: Mendapatkan nilai dari setiap sel dalam baris **/
                            for (int cell = 0; cell < 10; cell++)
                            {
                                var value = worksheet.GetRow(row)?.GetCell(cell)?.ToString(); // Mendapatkan nilai dari sel pada baris dan kolom tertentu
                                rowValues.Add(value);
                            }

                            /** Langkah 5: Membuat objek DTO siswa dari nilai setiap sel dalam baris **/
                            var studentDto = new RegisterStudentExcelDto
                            {
                                NameStudent = rowValues[0],
                                BirthDate = DateOnly.Parse(rowValues[1]),
                                BirthPlace = rowValues[2],
                                Address = rowValues[3],
                                PhoneNumber = rowValues[4],
                                Nis = rowValues[5],
                                ParentName = rowValues[6],
                                Gender = Convert.ToInt32(rowValues[7]),
                                UniqueNumberOfClassRoom = rowValues[8],
                                Username = rowValues[5], // Menggunakan NIS sebagai username
                                Password = $"{rowValues[5]}Edu#", // Menggunakan NIS sebagai basis password
                                Role = 3 // Tentukan peran default atau sesuaikan dengan kebutuhan Anda
                            };

                            /** Langkah 6: Validasi objek DTO siswa **/
                            var validationResult = await ValidateStudentDto(studentDto);
                            if (!validationResult.IsValid)
                            {
                                foreach (var error in validationResult.Errors)
                                {
                                    errors.Add($"Kesalahan dalam baris {row}: {error.ErrorMessage}");
                                }
                                continue; /** Langkah 6.1: Lewati iterasi saat ini jika validasi gagal **/
                            }

                            /** Langkah 7: Memeriksa duplikasi NIS **/
                            if (nisList.Contains(studentDto.Nis))
                            {
                                errors.Add($"Kesalahan dalam baris {row}: NIS {studentDto.Nis} sudah ada di baris sebelumnya");
                                continue; /** Langkah 7.1: Lewati iterasi saat ini jika NIS sudah ada sebelumnya **/
                            }

                            /** Langkah 7.2: Periksa duplikasi NIS di database **/
                            if (existingNisList.Contains(studentDto.Nis))
                            {
                                errors.Add($"Kesalahan dalam baris {row}: NIS {studentDto.Nis} sudah ada dalam database");
                                continue; // Skip the current iteration if NIS already exists in the database
                            }

                            /** Langkah 8: Menambahkan NIS ke HashSet **/
                            nisList.Add(studentDto.Nis);

                            /** Langkah 9: Menambahkan siswa yang lolos validasi ke daftar yang akan disimpan **/
                            studentsToSave.Add(studentDto);

                            /** Langkah 10: Mengosongkan daftar kesalahan jika baris valid baru ditemukan **/
                            errors.Clear();
                        }
                    }
                }

                /** Langkah 11: Jika tidak ada kesalahan, simpan semua siswa yang lolos validasi **/
                if (!errors.Any())
                {
                    foreach (var studentDto in studentsToSave)
                    {
                        var createUserResult = await CreateUserAndStudent(studentDto);
                        if (!createUserResult.IsSuccess)
                        {
                            errors.Add($"Kesalahan dalam baris: {createUserResult.Error}");
                            break; /** Langkah 11.1: Berhenti memproses jika terjadi kesalahan saat penyimpanan **/
                        }
                    }
                }

                /** Langkah 12: Kembalikan hasilnya **/
                if (errors.Any())
                {
                    return Result<List<RegisterStudentExcelDto>>.Failure(string.Join(", ", errors));
                }

                return Result<List<RegisterStudentExcelDto>>.Success(studentsToSave);
            }

            // Metode validasi DTO siswa
            private async Task<FluentValidation.Results.ValidationResult> ValidateStudentDto(RegisterStudentExcelDto studentDto)
            {
                var validator = new RegisterStudentCommandValidator();
                return await validator.ValidateAsync(studentDto);
            }

            // Metode untuk membuat pengguna dan siswa baru
            private async Task<Result<List<RegisterStudentExcelDto>>> CreateUserAndStudent(RegisterStudentExcelDto studentDto)
            {
                var validationResult = await ValidateStudentDto(studentDto);
                if (!validationResult.IsValid)
                {
                    return Result<List<RegisterStudentExcelDto>>.Failure(string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage)));
                }

                var user = _mapper.Map<AppUser>(studentDto);

                /** Langkah 13: Memeriksa duplikasi nama pengguna **/
                if (await _userManager.Users.AnyAsync(x => x.UserName == user.UserName))
                {
                    return Result<List<RegisterStudentExcelDto>>.Failure($"Username {user.UserName} sudah digunakan");
                }

                var student = _mapper.Map<Student>(studentDto);

                /** Langkah 14: Memeriksa duplikasi NIS **/
                if (await _context.Students.AnyAsync(s => s.Nis == student.Nis))
                {
                    return Result<List<RegisterStudentExcelDto>>.Failure($"Nis {student.Nis} sudah digunakan");
                }

                var selectedClass = await _context.ClassRooms.FirstOrDefaultAsync(c => c.UniqueNumberOfClassRoom == studentDto.UniqueNumberOfClassRoom);
                if (selectedClass == null)
                {
                    return Result<List<RegisterStudentExcelDto>>.Failure("Nomor UnikKelas yang dipilih tidak ditemukan");
                }

                student.AppUserId = user.Id;
                student.ClassRoomId = selectedClass.Id;

                var createUserResult = await _userManager.CreateAsync(user, studentDto.Password);

                /** Langkah 15: Memeriksa hasil pembuatan pengguna **/
                if (createUserResult.Succeeded)
                {
                    _context.Students.Add(student);
                    await _context.SaveChangesAsync();
                    return Result<List<RegisterStudentExcelDto>>.Success(new List<RegisterStudentExcelDto> { studentDto });
                }
                else
                {
                    return Result<List<RegisterStudentExcelDto>>.Failure(string.Join(", ", createUserResult.Errors.Select(e => e.Description)));
                }
            }
        }
    }