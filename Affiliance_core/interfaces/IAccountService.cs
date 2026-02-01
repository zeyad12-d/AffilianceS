using System.Threading.Tasks;
using Affiliance_core.ApiHelper;
using Affiliance_core.Dto.MarkterDto;

namespace Affiliance_core.interfaces
{
    public interface IAccountService
    {
        Task<ApiResponse<string>> RegisterMarketerAsync(MarketerRegisterDto dto);
        Task<ApiResponse<AuthModel>> LoginMarketerAsync(LoginMarkterDto dto);
    }
}
