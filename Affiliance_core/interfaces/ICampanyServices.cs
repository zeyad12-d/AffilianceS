using Affiliance_core.ApiHelper;
using Affiliance_core.Dto.CampanyDto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Affiliance_core.interfaces
{
    public interface ICampanyServices
    {
        Task<ApiResponse<string>> RegisterCompanyAsync(CompanyRegisterDto dto);
    }
}
