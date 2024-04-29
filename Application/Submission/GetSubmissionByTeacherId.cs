//using Application.Assignments;
//using Application.Core;
//using AutoMapper;
//using Domain.Class;
//using MediatR;
//using Microsoft.EntityFrameworkCore;
//using Persistence;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace Application.Submission
//{
//    public class GetSubmissionByTeacherId
//    {
//        public class Query : IRequest<Result<List<AssignmentDto>>>
//        {
//            public Guid TeacherId { get; set; }
//        }

//        public class Handler : IRequestHandler<Query, Result<List<AssignmentDto>>>
//        {
//            private readonly DataContext _context;
//            private readonly IMapper _mapper;

//            public Handler(DataContext context, IMapper mapper)
//            {
//                _context = context;
//                _mapper = mapper;
//            }

//            public async Task<Result<List<AssignmentDto>>> Handle(Query request, CancellationToken cancellationToken)
//            {
//                try
//                {
//                    // Mendapatkan ClassRooms yang diajar oleh guru dengan TeacherId tertentu
//                    var classRooms = await _context.TeacherClassRooms
//                        .Where(tc => tc.TeacherId == request.TeacherId)
//                        .Select(tc => tc.ClassRoom)
//                        .ToListAsync(cancellationToken);

//                    // Mendapatkan Assignment yang terkait dengan ClassRooms yang diajar oleh guru
//                    var assignments = await _context.Assignments
//                        .Where(a => classRooms.Any(cr => cr.CourseClassRooms.Any(ccr => ccr.CourseId == a.CourseId)))
//                        .ToListAsync(cancellationToken);

//                    // Memetakan entitas Assignment ke AssignmentDto
//                    var assignmentDtos = _mapper.Map<List<AssignmentDto>>(assignments);

//                    return Result<List<AssignmentDto>>.Success(assignmentDtos);
//                }
//                catch (Exception ex)
//                {
//                    // Menangani pengecualian dan mengembalikan pesan kesalahan yang sesuai
//                    return Result<List<AssignmentDto>>.Failure($"Failed to get assignments: {ex.Message}");
//                }
//            }
//        }
//    }
//}