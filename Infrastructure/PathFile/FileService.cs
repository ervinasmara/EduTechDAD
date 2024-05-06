using Application.Interface;
using Microsoft.AspNetCore.Http;

namespace Infrastructure.PathFile;
public class FileService : IFileService
{
    public async Task<string> SaveFileAsync(IFormFile file, string relativeFolderPath, string assignmentName, DateTime createdAt)
    {
        try
        {
            // Konversi waktu sekarang ke zona waktu Indonesia Barat (WIB, UTC+7)
            DateTime localDateTime = DateTime.UtcNow.AddHours(7);

            // Buat nama file berdasarkan waktu dan nama tugas
            string fileName = $"{localDateTime:yyyyMMdd_HHmmss}_{assignmentName}_{Path.GetFileName(file.FileName)}";

            // Path lengkap untuk menyimpan file
            string filePath = Path.Combine(relativeFolderPath, fileName);

            // Buat folder jika belum ada
            Directory.CreateDirectory(Path.GetDirectoryName(filePath));

            // Simpan file ke dalam folder
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            // Jika perlu, bisa mengembalikan informasi tambahan seperti path file yang disimpan
            return filePath;
        }
        catch (Exception ex)
        {
            // Menangani error jika terjadi kesalahan saat menyimpan file
            throw new Exception("Gagal menyimpan file.", ex);
        }
    }

    public async Task<string> SaveFileSubmission(IFormFile file, string relativeFolderPath, DateTime submissionTime)
    {
        try
        {
            // Konversi waktu sekarang ke zona waktu Indonesia Barat (WIB, UTC+7)
            DateTime localDateTime = DateTime.UtcNow.AddHours(7);

            // Buat nama file berdasarkan waktu dan nama tugas
            string fileName = $"{localDateTime:yyyyMMdd_HHmmss}_{Path.GetFileName(file.FileName)}";

            // Path lengkap untuk menyimpan file
            string filePath = Path.Combine(relativeFolderPath, fileName);

            // Buat folder jika belum ada
            Directory.CreateDirectory(Path.GetDirectoryName(filePath));

            // Simpan file ke dalam folder
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            // Jika perlu, bisa mengembalikan informasi tambahan seperti path file yang disimpan
            return filePath;
        }
        catch (Exception ex)
        {
            // Menangani error jika terjadi kesalahan saat menyimpan file
            throw new Exception("Gagal menyimpan file.", ex);
        }
    }
}