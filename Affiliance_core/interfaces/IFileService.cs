using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Affiliance_core.interfaces
{
    public interface IFileService
    {
        Task<string> SaveFileAsync(IFormFile file, string folderName);
        Task<string> UploadFileAsync(IFormFile file, string folderName);
        Task DeleteFileAsync(string filePath);
    }
}
