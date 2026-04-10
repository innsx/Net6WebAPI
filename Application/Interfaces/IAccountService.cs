using Application.DTOs;
using Application.Wrappers;

namespace Application.Interfaces
{
    public interface IAccountService
    {
        Task<CustomizedAPIResponse<Guid>> RegisterUser(RegisterUserDto registerUserDto);

        Task <CustomizedAPIResponse<AuthnResponseDto>> Authenticate(AuthnRequestDto authenticationRequest);

        Task<CustomizedAPIResponse<bool>> ConfirmEmail(string userId, string token);

        Task<CustomizedAPIResponse<bool>> ReSendConfirmationEmailAsync(string email);

        Task<CustomizedAPIResponse<bool>> ResetUserPasswordAsync(ResetPasswordRequestDto resetPasswordRequestDto);

        Task<CustomizedAPIResponse<bool>> ForgotPasswordAsync(string userEmail);
    }
}