namespace Application.Learn.GetFileName;
public static class FileHelper
{
    public static string GetContentType(string fileExtension)
    {
        switch (fileExtension.ToLower())
        {
            case "jpg":
            case "jpeg":
                return "image/jpeg";
            case "png":
                return "image/png";
            case "pdf":
                return "application/pdf";
            case "zip":
                return "application/zip";
            case "rar":
                return "application/vnd.rar";
            default:
                return "application/octet-stream";
        }
    }
}