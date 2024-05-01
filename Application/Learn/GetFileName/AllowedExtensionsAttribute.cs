using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace Application.Learn.GetFileName
{
    public class AllowedExtensionsAttribute : ValidationAttribute
    {
        private readonly string[] _extensions;

        public AllowedExtensionsAttribute(string[] extensions)
        {
            _extensions = extensions;
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            // Konversi nilai menjadi IFormFile
            var file = value as IFormFile;

            // Periksa apakah nilai yang diterima adalah file
            if (file != null)
            {
                // Dapatkan ekstensi file dari nama file
                var extension = Path.GetExtension(file.FileName);

                // Periksa apakah ekstensi file ada di dalam daftar ekstensi yang diizinkan
                if (!_extensions.Contains(extension.ToLower()))
                {
                    // Jika ekstensi tidak diizinkan, kembalikan pesan kesalahan validasi
                    return new ValidationResult(GetErrorMessage());
                }
            }

            // Jika ekstensi file sesuai dengan yang diizinkan atau nilai tidak ada, kembalikan hasil validasi yang berhasil
            return ValidationResult.Success;
        }

        public string GetErrorMessage()
        {
            return $"Only files with the following extensions are allowed: {string.Join(", ", _extensions)}";
        }
    }
}
