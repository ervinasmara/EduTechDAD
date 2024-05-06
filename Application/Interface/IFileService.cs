using Microsoft.AspNetCore.Http;

namespace Application.Interface;
public interface IFileService
{
    Task<string> SaveFileAsync(IFormFile file, string relativeFolderPath, string assignmentName, DateTime createdAt);
    Task<string> SaveFileSubmission(IFormFile fileData, string relativeFolderPath, DateTime submissionTime);
}