namespace Application.Learn.GetFileName;
public static class GetFileExtension
{
    public static string FileExtensionHelper(byte[] fileData)
    {
        if (fileData == null || fileData.Length < 4)
            return null;

        // Analisis byte pertama untuk menentukan jenis file
        if (fileData[0] == 0xFF && fileData[1] == 0xD8 && fileData[2] == 0xFF)
        {
            return "jpg";
        }
        else if (fileData[0] == 0x89 && fileData[1] == 0x50 && fileData[2] == 0x4E && fileData[3] == 0x47)
        {
            return "png";
        }
        else if (fileData[0] == 0x25 && fileData[1] == 0x50 && fileData[2] == 0x44 && fileData[3] == 0x46)
        {
            return "pdf";
        }
        else if (fileData[0] == 0x50 && fileData[1] == 0x4B && fileData[2] == 0x03 && fileData[3] == 0x04)
        {
            return "zip";
        }
        else if (fileData[0] == 0x52 && fileData[1] == 0x61 && fileData[2] == 0x72 && fileData[3] == 0x21)
        {
            return "rar";
        }
        else
        {
            return null; // Ekstensi file tidak dikenali
        }
    }
}