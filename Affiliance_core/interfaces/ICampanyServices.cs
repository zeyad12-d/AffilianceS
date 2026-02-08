using Affiliance_core.ApiHelper;
using Affiliance_core.Dto.AccountDto;
using Affiliance_core.Dto.MarkterDto;

namespace Affiliance_core.interfaces
{
    public interface ICampanyServices
    {
        Task<ApiResponse<string>> RegisterCompanyAsync(CompanyRegisterDto dto);

       
    }
}
