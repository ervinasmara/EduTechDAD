using Microsoft.AspNetCore.Http;

namespace Application.Submission
{
    /** //////////////////////////////////////// **/
    /** STUDENT **/
    public class SubmissionCreateByStudentIdDto
    {
        public Guid AssignmentId { get; set; }
        public IFormFile FileData { get; set; }
        public string Link { get; set; }
    }

    public class SubmissionEditByStudentIdDto
    {
        public IFormFile FileData { get; set; }
        public string Link { get; set; }
    }

    public class AssignmentSubmissionGetByAssignmentIdAndStudentId
    {
        public Guid Id { get; set; }
        public DateTime SubmissionTime { get; set; }
        public string Status { get; set; }
        public string SubmissionTimeStatus { get; set; }
        public string Link { get; set; }
        public float Grade { get; set; }
        public string Comment { get; set; }
        public string FileData { get; set; }
    }

    /** //////////////////////////////////////// **/
    /** Get DTO For Teacher Grade And Not Submitted **/
    public class NotSubmittedDto
    {
        public Guid StudentId { get; set; }
        public string StudentName { get; set; }
        public string ClassName { get; set; }
        public string LongClassName { get; set; }
    }

    public class AssignmentSubmissionTeacherDto
    {
        public float Grade { get; set; }
        public string Comment { get; set; }
    }

    public class AssignmentSubmissionListGradeDto
    {
        public Guid Id { get; set; }
        public DateTime SubmissionTime { get; set; }
        public string Status { get; set; }
        public string SubmissionTimeStatus { get; set; }
        public string Link { get; set; }
        public float Grade { get; set; }
        public string Comment { get; set; }
        public string AssignmentId { get; set; }
        public string AssignmentName { get; set; }
        public string StudentId { get; set; }
        public string StudentName { get; set; }
        public string ClassName { get; set; }
        public string FileData { get; set; }
    }

    public class AssignmentSubmissionListForTeacherGradeDto
    {
        public string AlreadyGrades { get; set; }
        public string NotAlreadyGrades { get; set; }
        public string NotYetSubmit { get; set; }
        public ICollection<AssignmentSubmissionListGradeDto> AssignmentSubmissionList { get; set; }
        public ICollection<NotSubmittedDto> StudentNotYetSubmit { get; set; }
    }
}
