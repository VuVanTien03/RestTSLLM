using HotelListing.API.Core.Contracts;
using HotelListing.API.Core.Models;
using HotelListing.API.Core.Models.Users;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace HotelListing.API.Controllers
{
    [Route("api/accounts")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly IAuthManager _authManager;
        private readonly ILogger<AccountController> _logger;

        public AccountController(IAuthManager authManager, ILogger<AccountController> logger)
        {
            this._authManager = authManager;
            this._logger = logger;
        }

        // POST: api/accounts
        [HttpPost]
        [Route("")]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(BadRequestResponse))]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(void))]
        public async Task<ActionResult> Register([FromBody] ApiUserDto apiUserDto)
        {
            _logger.LogInformation($"Registration Attempt for {apiUserDto.Email}");
            var errors = await _authManager.Register(apiUserDto);

            if (errors.Any())
            {
                var errorsList = new Dictionary<string, string[]>();
                foreach (var error in errors)
                {
                    errorsList.Add(error.Code, new string[] { error.Description });
                }
                return BadRequest(new BadRequestResponse(errorsList, Activity.Current.Id));
            }

            return Ok();
        }

        // POST: api/accounts/tokens
        [HttpPost]
        [Route("tokens")]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(BadRequestResponse))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(void))]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(AuthResponseDto))]
        public async Task<ActionResult> Login([FromBody] LoginDto loginDto)
        {
            _logger.LogInformation($"Login Attempt for {loginDto.Email} ");
            var authResponse = await _authManager.Login(loginDto);

            if (authResponse == null)
            {
                return Unauthorized(null);
            }

            return Ok(authResponse);

        }

        // POST: api/accounts/refreshtokens
        [HttpPost]
        [Route("refreshtokens")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(AuthResponseDto))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(BadRequestResponse))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(void))]
        public async Task<ActionResult> RefreshToken([FromBody] AuthResponseDto request)
        {
            try
            {
                var authResponse = await _authManager.VerifyRefreshToken(request);

                if (authResponse == null)
                {
                    return Unauthorized(null);
                }

                return Ok(authResponse);
            }
            catch (Exception)
            {
                return Unauthorized(null);
            }
        }
    }
}
