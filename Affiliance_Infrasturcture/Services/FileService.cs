using System;
using System.IO;
using System.Threading.Tasks;
using Affiliance_core.interfaces;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http; // Ensure IFormFile usage

namespace Affiliance_Infrasturcture.Services
{
    // Interface is now in Core layer, removed here
    public class FileService : IFileService
    {
        private readonly IWebHostEnvironment _env;

        public FileService(IWebHostEnvironment env)
        {
            _env = env;
        }

        public async Task<string> SaveFileAsync(IFormFile file, string folderName)
        {
            if (file == null || file.Length == 0)
                throw new ArgumentNullException(nameof(file));

            var uploadsFolder = Path.Combine(_env.WebRootPath ?? _env.ContentRootPath, "uploads", folderName);
            
            if (!Directory.Exists(uploadsFolder))
                Directory.CreateDirectory(uploadsFolder);

            var uniqueFileName = Guid.NewGuid().ToString() + "_" + file.FileName;
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(fileStream);
            }

            // Return relative path for database storage
            return Path.Combine("uploads", folderName, uniqueFileName).Replace("\\", "/");
        }

        public async Task<string> UploadFileAsync(IFormFile file, string folderName)
        {
            return await SaveFileAsync(file, folderName);
        }

        public Task DeleteFileAsync(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
            {
                return Task.CompletedTask;
            }

            var normalizedPath = filePath.TrimStart('~', '/').Replace("/", Path.DirectorySeparatorChar.ToString());
            var fullPath = Path.Combine(_env.WebRootPath ?? _env.ContentRootPath, normalizedPath);

            if (File.Exists(fullPath))
            {
                File.Delete(fullPath);
            }

            return Task.CompletedTask;
        }
    }
}
