using System.Threading.Tasks;
using Affiliance_core.Dto;
using Affiliance_core.interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Affiliance_Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly IAccountService _accountService;

        public AccountController(IAccountService accountService)
        {
            _accountService = accountService;
        }

        [HttpPost("register-marketer")]
        public async Task<IActionResult> RegisterMarketer([FromForm] MarketerRegisterDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _accountService.RegisterMarketerAsync(dto);

            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }
    }
}
