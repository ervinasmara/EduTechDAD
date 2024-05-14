using Application.Core;
using Application.Interface;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Assignments.Query;
public class ListAssignmentsByClassRoomId
{
    public class Query : IRequest<Result<List<AssignmentGetByClassRoomIdDto>>>
    {
        public Guid ClassRoomId { get; set; }
    }

    public class Handler : IRequestHandler<Query, Result<List<AssignmentGetByClassRoomIdDto>>>
    {
        private readonly DataContext _context;
        private readonly IMapper _mapper;
        private readonly IUserAccessor _userAccessor;

        public Handler(DataContext context, IMapper mapper, IUserAccessor userAccessor)
        {
            _context = context;
            _mapper = mapper;
            _userAccessor = userAccessor;
        }

        public async Task<Result<List<AssignmentGetByClassRoomIdDto>>> Handle(Query request, CancellationToken cancellationToken)
        {
            /** Langkah 1: Dapatkan ClassRoomId dari token (penanganan kesalahan untuk format yang tidak valid) **/
            var userId = _userAccessor.GetClassRoomIdFromToken();
            if (!Guid.TryParse(userId, out var parsedUserId))
            {
                return Result<List<AssignmentGetByClassRoomIdDto>>.Failure("Invalid ClassRoomId");
            }

            /** Langkah 2: Membuat query untuk penugasan berdasarkan ClassRoomId dan status aktif **/
            var assignments = await _context.Assignments
                .Where(a => a.Course.Lesson.ClassRoomId == parsedUserId && a.Status != 0) // Kecualikan tugas dengan status 0 (tidak aktif)
                .OrderByDescending(a => a.CreatedAt) // Urutkan berdasarkan tanggal pembuatan (menurun)
                .ProjectTo<AssignmentGetByClassRoomIdDto>(_mapper.ConfigurationProvider) // Memetakan hasil secara efisien ke DTO menggunakan AutoMapper
                .ToListAsync(cancellationToken);

            /** 3. Periksa hasil kosong dan kembalikan kegagalan jika tidak ada penugasan yang ditemukan **/
            if (!assignments.Any())
            {
                return Result<List<AssignmentGetByClassRoomIdDto>>.Failure("Tidak ada tugas yang ditemukan untuk ClassRoomId yang diberikan");
            }

            /** Langkah 4: Ulangi penugasan dan tambahkan status pengajuan ke setiap DTO **/
            foreach (var dto in assignments)
            {
                /** Langkah 4.1: Dapatkan StudentId dari token (penanganan kesalahan untuk format yang tidak valid) **/
                var studentId = _userAccessor.GetStudentIdFromToken();
                if (!Guid.TryParse(studentId, out var parsedStudentId))
                {
                    dto.AssignmentSubmissionStatus = "Invalid StudentId";
                    continue; // Lewati ke iterasi berikutnya
                }

                /** Langkah 4.2: Menemukan AssignmentSubmission untuk Student dan Assignment saat ini **/
                var submission = await _context.AssignmentSubmissions
                    .FirstOrDefaultAsync(s => s.AssignmentId == dto.Id && s.StudentId == parsedStudentId, cancellationToken);

                /** Langkah 4.3: Mengatur status Assignment berdasarkan AssignmentDeadline dan AssignmentSubmission **/
                dto.AssignmentSubmissionStatus = submission == null
                    ? DateTime.UtcNow.AddHours(7) > dto.AssignmentDeadline ? "Terlambat Mengerjakan" : "Belum Mengerjakan"
                    : submission.Status switch
                    {
                        1 => "Sudah mengerjakan",
                        2 => "Sudah dinilai",
                        _ => "Status tidak diketahui",
                    };
            }

            /** Langkah 5: Kembalikan hasil yang berhasil dengan Assignments dan Submission statuses **/
            return Result<List<AssignmentGetByClassRoomIdDto>>.Success(assignments);
        }
    }
}