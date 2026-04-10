using Application.DTOs;
using Application.Exceptions;
using Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers
{
    //Video: https://www.youtube.com/watch?v=-TIMkYGO9RU&list=PLyTjFFFANHHfPsdxw_BX5IxK0Ayk-fCWv&index=8

    [Route("api/[controller]")]
    [ApiController]
    public class AccountsController : ControllerBase
    {
        private readonly IAccountService _accountService;

        public AccountsController(IAccountService accountService)
        {
            _accountService = accountService;
        }

        [HttpPost("RegisterUser")]
        [ProducesResponseType(StatusCodes.Status200OK)]   
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]     
        public async Task<IActionResult> RegisterUser(RegisterUserDto registerUserDto, CancellationToken cancellationToken)
        {
            var result = await _accountService.RegisterUser(registerUserDto);
            return Ok(result);
        }

        [HttpPost("Login")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Authentication(AuthnRequestDto authModel, CancellationToken cancellationToken)
        {
            var result = await _accountService.Authenticate(authModel);
            return Ok(result);
        }


        [HttpGet("confirm-email")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ConfirmEmail([FromQuery] string userId, string token, CancellationToken cancellationToken)
        {
            var result = await _accountService.ConfirmEmail(userId, token);
            return Ok(result);
        }

        [HttpGet("resend-confirm-email")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ResendConfirmEmail(string email, CancellationToken cancellationToken)
        {
            var result = await _accountService.ReSendConfirmationEmailAsync(email);
            return Ok(result);
        }

        [HttpGet("forgot-password")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> ForgotPassword([FromQuery] string email, CancellationToken cancellationToken)
        {
            var result = await _accountService.ForgotPasswordAsync(email);
            return Ok(result);
        }


        [HttpPost("reset-password")]       
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> ResetPassword(ResetPasswordRequestDto resetPasswordRequestDto, CancellationToken cancellationToken)
        {
            var result = await _accountService.ResetUserPasswordAsync(resetPasswordRequestDto);
            return Ok(result);
        }

    }
}
