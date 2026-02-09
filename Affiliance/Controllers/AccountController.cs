using System.Security.Claims;
using System.Threading.Tasks;
using Affiliance_core.Dto.AccountDto;
using Affiliance_core.interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Affiliance_Api.Controllers
{
    /// <summary>
    /// Controller for managing user account operations including registration, login, and logout.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly IAccountService _accountService;
        private readonly IServicesManager _servicesManager;

        /// <summary>
        /// Initializes a new instance of the <see cref="AccountController"/> class.
        /// </summary>
        /// <param name="accountService">The account service for handling account operations.</param>
        public AccountController(IAccountService accountService, IServicesManager servicesManager)
        {
            _accountService = accountService;
            _servicesManager = servicesManager;

        }

        /// <summary>
        /// Registers a new marketer account.
        /// </summary>
        /// <param name="dto">The marketer registration data including email, password, full name, phone number, and national ID image.</param>
        /// <returns>
        /// Returns <see cref="OkObjectResult"/> with the user ID on successful registration,
        /// or <see cref="BadRequestObjectResult"/> if registration fails.
        /// </returns>
        /// <response code="200">Marketer registered successfully.</response>
        /// <response code="400">Invalid input or registration failed.</response>
        [HttpPost("register-marketer")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
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

        /// <summary>
        /// Authenticates a marketer,Company and returns JWT and refresh tokens.
        /// </summary>
        /// <param name="dto">The login credentials containing email and password.</param>
        /// <returns>
        /// Returns <see cref="OkObjectResult"/> with authentication tokens on successful login,
        /// or <see cref="BadRequestObjectResult"/> if login fails.
        /// </returns>
        /// <response code="200">Login successful. Returns JWT token and refresh token.</response>
        /// <response code="400">Invalid credentials or login failed.</response>
        [HttpPost("login")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _accountService.LoginMarketerAsync(dto);

            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }

        /// <summary>
        /// Logs out the currently authenticated user by invalidating their refresh token.
        /// </summary>
        /// <returns>
        /// Returns <see cref="OkObjectResult"/> on successful logout,
        /// <see cref="UnauthorizedResult"/> if user is not authenticated,
        /// or <see cref="BadRequestObjectResult"/> if logout fails.
        /// </returns>
        /// <response code="200">User logged out successfully.</response>
        /// <response code="401">User is not authenticated.</response>
        /// <response code="400">Logout operation failed.</response>
        [Authorize]
        [HttpPost("logout")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Logout()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var result = await _accountService.LogoutAsync(userId);

            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }


        [HttpPost("campany_Register")]
        public async Task<IActionResult> RegisterCampany([FromForm] CompanyRegisterDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var result = await _servicesManager.CampanyServices.RegisterCompanyAsync(dto);
            if (!result.Success)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }
        [HttpPost("ChangePassword")]
        public async Task<IActionResult> ChangePasswordAsync([FromBody] ChangePasswordDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var result = await _accountService.ChangePasswordAsync(dto);
            if(!result.Success)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }


        // you must make admin services to git all campanys registerd to make it approve or reject 
        ///remmber to make migration for new changes
    }
}
