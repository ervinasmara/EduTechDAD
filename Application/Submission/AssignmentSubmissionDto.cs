using Microsoft.AspNetCore.Http;

namespace Application.Submission
{
    /** //////////////// STUDENT //////////////////////// **/
    /** Create Submission For Student **/
    public class SubmissionCreateByStudentIdDto
    {
        public Guid AssignmentId { get; set; }
        public IFormFile FileData { get; set; }
        public string Link { get; set; }
    }

    /** Get Submission By AssignmentId For Student **/
    public class AssignmentSubmissionGetByAssignmentIdAndStudentId
    {
        public Guid Id { get; set; }
        public DateTime SubmissionTime { get; set; }
        public string Status { get; set; }
        public string SubmissionTimeStatus { get; set; }
        public string Link { get; set; }
        public decimal Grade { get; set; }
        public string Comment { get; set; }
        public string FileData { get; set; }
    }

    /** Get Submission By SubmissionId For Teacher **/
    public class AssignmentSubmissionGetBySubmissionIdAndTeacherId
    {
        public Guid Id { get; set; }
        public string NameStudent { get; set; }
        public string AssignmentName { get; set; }
        public DateTime SubmissionTime { get; set; }
        public string Status { get; set; }
        public string SubmissionTimeStatus { get; set; }
        public string Link { get; set; }
        public decimal Grade { get; set; }
        public string Comment { get; set; }
        public string FileName { get; set; }
        public string FileData { get; set; }
    }

    /** //////////////// TEACHER //////////////////////// **/
    /** Get Not Submitted **/
    public class NotSubmittedDto
    {
        public Guid StudentId { get; set; }
        public string StudentName { get; set; }
        public string ClassName { get; set; }
        public string LongClassName { get; set; }
    }

    /** Edit Submission For Teacher Grades **/
    public class AssignmentSubmissionTeacherDto
    {
        public decimal Grade { get; set; }
        public string Comment { get; set; }
    }

    /** List Submission For Teacher **/
    public class AssignmentSubmissionListGradeDto
    {
        public Guid Id { get; set; }
        public DateTime SubmissionTime { get; set; }
        public string Status { get; set; }
        public string SubmissionTimeStatus { get; set; }
        public string Link { get; set; }
        public decimal Grade { get; set; }
        public string Comment { get; set; }
        public string AssignmentId { get; set; }
        public string AssignmentName { get; set; }
        public string StudentId { get; set; }
        public string StudentName { get; set; }
        public string ClassName { get; set; }
        public string FileData { get; set; }
    }

    /** List Submission For Teacher **/
    public class AssignmentSubmissionListForTeacherGradeDto
    {
        public string AlreadyGrades { get; set; }
        public string NotAlreadyGrades { get; set; }
        public string NotYetSubmit { get; set; }
        public ICollection<AssignmentSubmissionListGradeDto> AssignmentSubmissionList { get; set; }
        public ICollection<NotSubmittedDto> StudentNotYetSubmit { get; set; }
    }
}
