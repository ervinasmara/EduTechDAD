//using Application.Core;
//using Application.Interface;
//using AutoMapper;
//using Domain.Submission;
//using MediatR;
//using Microsoft.EntityFrameworkCore;
//using Persistence;

//namespace Application.Submission
//{
//    public class ListSubmissionByTeacherId
//    {
//        public class Query : IRequest<Result<List<AssignmentSubmissionGetDto>>>
//        {
//            public string ClassName { get; set; }
//            public string AssignmentName { get; set; }

//            public Query(string className, string assignmentName)
//            {
//                ClassName = className;
//                AssignmentName = assignmentName;
//            }
//        }

//        public class Handler : IRequestHandler<Query, Result<List<AssignmentSubmissionGetDto>>>
//        {
//            private readonly IUserAccessor _userAccessor;
//            private readonly DataContext _context;
//            private readonly IMapper _mapper;

//            public Handler(IUserAccessor userAccessor, DataContext context, IMapper mapper)
//            {
//                _userAccessor = userAccessor;
//                _context = context;
//                _mapper = mapper;
//            }

//            public async Task<Result<List<AssignmentSubmissionGetDto>>> Handle(Query request, CancellationToken cancellationToken)
//            {
//                // Mendapatkan TeacherId dari token dan mengonversi ke Guid
//                var teacherId = Guid.Parse(_userAccessor.GetTeacherIdFromToken());

//                // Validasi ClassName
//                var teacherClassroom = await _context.TeacherClassRooms
//                    .Include(tc => tc.ClassRoom)
//                    .FirstOrDefaultAsync(tc => tc.TeacherId == teacherId && tc.ClassRoom.ClassName == request.ClassName);

//                if (teacherClassroom == null)
//                    return Result<List<AssignmentSubmissionGetDto>>.Failure("Teacher does not have the specified class.");

//                // Validasi AssignmentName
//                var teacherAssignment = await _context.TeacherAssignments
//                    .Include(ta => ta.Assignment)
//                    .FirstOrDefaultAsync(ta => ta.TeacherId == teacherId && ta.Assignment.AssignmentName == request.AssignmentName);

//                if (teacherAssignment == null)
//                    return Result<List<AssignmentSubmissionGetDto>>.Failure("Teacher does not have the specified assignment.");

//                // Mengambil AssignmentSubmission berdasarkan AssignmentName
//                var assignmentSubmissions = await _context.AssignmentSubmissions
//                    .Where(asb => asb.AssignmentId == teacherAssignment.AssignmentId)
//                    .ToListAsync(cancellationToken);

//                // Mengonversi AssignmentSubmission menjadi DTO
//                var assignmentSubmissionDtos = _mapper.Map<List<AssignmentSubmissionGetDto>>(assignmentSubmissions);

//                // Ambil ClassName berdasarkan ClassRoomId yang terkait dengan setiap AssignmentSubmission
//                foreach (var dto in assignmentSubmissionDtos)
//                {
//                    var student = await _context.Students.FindAsync(Guid.Parse(dto.StudentId));
//                    if (student != null)
//                    {
//                        var classRoom = await _context.ClassRooms.FindAsync(student.ClassRoomId);
//                        if (classRoom != null)
//                        {
//                            dto.ClassName = classRoom.ClassName;
//                        }
//                    }
//                }

//                return Result<List<AssignmentSubmissionGetDto>>.Success(assignmentSubmissionDtos);
//            }
//        }
//    }
//}