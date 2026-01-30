using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace Affiliance_core.interfaces
{
    public interface IAiService
    {
        Task<string> AnalyzeImageAsync(IFormFile image);
    }
}
