using System.Threading.Tasks;
using Affiliance_core.ApiHelper;
using Affiliance_core.Dto.AccountDto;

namespace Affiliance_core.interfaces
{
    public interface IAccountService
    {
        Task<ApiResponse<string>> RegisterMarketerAsync(MarketerRegisterDto dto);
        Task<ApiResponse<AuthModel>> LoginMarketerAsync(LoginDto dto);
        Task<ApiResponse<bool>> LogoutAsync(string userId);
        Task<ApiResponse<bool>> ChangePasswordAsync(ChangePasswordDto dto);
        
    }
}
