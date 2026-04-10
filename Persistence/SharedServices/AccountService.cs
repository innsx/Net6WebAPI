using Application.DTOs;
using Application.Enums;
using Application.Exceptions;
using Application.Interfaces;
using Application.Wrappers;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Persistence.IdentityModels;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Persistence.SharedServices
{
    public class AccountService : IAccountService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IConfiguration _configuration;
        private readonly IEmailService _emailService;

        //ASP.NET Core injects the UserManager<TUser> service to manage user-related operations
        //like creation, updates, and authentication.
        //It acts as a wrapper around the framework's identity system,
        //enabling custom logic for registration,
        //password validation, and user claims.
        public AccountService(UserManager<ApplicationUser> userManager, IConfiguration configuration, IEmailService emailService)
        {
            _userManager = userManager;
            _configuration = configuration;
            _emailService = emailService;
        }

        public async Task<CustomizedAPIResponse<Guid>> RegisterUser(RegisterUserDto registerUserDto)
        {
            //User Management: The service uses the injected userManager to call methods
            //like CreateAsync, FindByNameAsync, FindByEmailAsync or AddToRoleAsync
            var email = await _userManager.FindByEmailAsync(registerUserDto.Email);

            if (email is not null)
            {
                throw new ApiErrorExceptions($"{registerUserDto.Email} is areadly taken.");
            }

            var newUser = new ApplicationUser();
            newUser.UserName = registerUserDto.UserName;
            newUser.Email = registerUserDto.Email;
            newUser.EmailConfirmed = true;
            newUser.FirstName = registerUserDto.FirstName;
            newUser.LastName = registerUserDto.LastName;
            newUser.Gender = registerUserDto.Gender;
            newUser.PhoneNumberConfirmed = true;

            var identityResult = await _userManager.CreateAsync(newUser, registerUserDto.Password);

            if (identityResult.Succeeded)
            {
                await _userManager.AddToRoleAsync(newUser, Roles.Basic.ToString());

                await SendConfirmationEmailAsync(newUser);

                return new CustomizedAPIResponse<Guid>(newUser.Id, "Email Sent successfully.");
            }
            else
            {
                throw new ApiErrorExceptions(identityResult.Errors.ToString()!);
            }
        }

        private async Task SendConfirmationEmailAsync(ApplicationUser newUser)
        {
            string token = await _userManager.GenerateEmailConfirmationTokenAsync(newUser);

            token = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));

            string verificationUrl = $"{_configuration["ClientUrl"]}/api/accounts/confirm-email?userId={newUser.Id}&token={token}";

            var emailRequest = new EmailRequestDto()
            {
                To = newUser.Email,
                Body = $"<p>Please verify your account by click on this link: { verificationUrl } </p> <br> <p>If this email is not your, ignore it.</p>",
                Subject = $"Confirm email {newUser.Email} to Admin",
                IsHtmlBody = true,
            };

            await _emailService.SendAsync(emailRequest);
        }

        public async Task<CustomizedAPIResponse<bool>> ConfirmEmail(string userId, string token)
        {
            var user = await _userManager.FindByIdAsync(userId);

            if (user is null)
            {
                throw new ApiErrorExceptions($"User is not found with this Id: {userId}");
            }

            token = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(token));

            var identityResult = await _userManager.ConfirmEmailAsync(user, token);

            if (identityResult.Succeeded)
            {
                return new CustomizedAPIResponse<bool>(true, "Email confirmed successfully.");
            }
            else
            {
                // ! is the null-forgiving operator. It asserts to the compiler that the value result.Errors is not null,
                // suppressing potential null warnings, but it will not prevent a runtime exception if the object is,
                // in fact, null.
                throw new ApiErrorExceptions(identityResult.Errors.ToString()!);
            }
        }

        public async Task<CustomizedAPIResponse<bool>> ReSendConfirmationEmailAsync(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);

            if (user == null)
            {
                throw new ApiErrorExceptions($"Unable to send confirm email to: {email}");
            }

            if (user.EmailConfirmed)
            {
                throw new ApiErrorExceptions("Email is already confirmed.");
            }

            await SendConfirmationEmailAsync(user);

            return new CustomizedAPIResponse<bool>(true, "Email Confirmation sent to your email account, please verify your account");
        }

        public async Task<CustomizedAPIResponse<AuthnResponseDto>> Authenticate(AuthnRequestDto authnRequestDto)
        {
            var user = await _userManager.FindByEmailAsync(authnRequestDto.Email);

            if (user is null)
            {
                throw new ApiErrorExceptions($"User is not registerd with this email {authnRequestDto.Email}.");
            }

            if (user.EmailConfirmed == false)
            {
                throw new ApiErrorExceptions($"User not confirmed, please confirm your email to login");
            }

            if (await _userManager.IsLockedOutAsync(user) == true)
            {
                throw new ApiErrorExceptions($"Your account is locked. Please try login later.");
            }
             
            var isPasswordValid = await _userManager.CheckPasswordAsync(user, authnRequestDto.Password);

            if (isPasswordValid == false)
            {
                await _userManager.AccessFailedAsync(user);
                throw new ApiErrorExceptions("Email or Password is incorrected.");
            }

            var authResponseDto = new AuthnResponseDto();

            authResponseDto.Id = user.Id;
            authResponseDto.UserName = user.UserName;
            authResponseDto.Email = user.Email;
            authResponseDto.IsVerified = user.EmailConfirmed;

            var roles = await _userManager.GetRolesAsync(user);
            authResponseDto.Roles = roles.ToList();

            var jwtSecurityToken = await GenerateTokenAsync(user);

            authResponseDto.JWToken = new JwtSecurityTokenHandler().WriteToken(jwtSecurityToken);

            return new CustomizedAPIResponse<AuthnResponseDto>(authResponseDto, "User is Authenticated.");
        }

        private async Task<JwtSecurityToken> GenerateTokenAsync(ApplicationUser applicationUser)
        {
            var dbClaimsList = await _userManager.GetClaimsAsync(applicationUser!);
            var roles = await _userManager.GetRolesAsync(applicationUser!);

            var rolesFromClaims = new List<Claim>();

            for (int i = 0; i < roles.Count; i++)
            {
                rolesFromClaims.Add(new Claim("roles", roles[i]));
            }

            string ipAddress = "192.33.123";

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, applicationUser.UserName),  //Sub means Subject
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),  //jti means Jwt Token Id
                new Claim(JwtRegisteredClaimNames.Email, applicationUser.Email),
                new Claim("uid", applicationUser.Id.ToString()),
                new Claim("ip", ipAddress)
            }
            .Union(dbClaimsList)
            .Union(rolesFromClaims);

            var symmetricSecurityKey = 
                    new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JwtSettings:Key"]));

            var signingCredentials = 
                new SigningCredentials(symmetricSecurityKey, SecurityAlgorithms.HmacSha256);

            var jwtSecurityToken = new JwtSecurityToken(
                    issuer: _configuration["JwtSettings:Issuer"],
                    audience: _configuration["JwtSettings:Audience"],
                    claims: claims,
                    expires: DateTime.UtcNow.AddMinutes(Convert.ToInt32(_configuration["JwtSettings:DurationInMinutes"])),
                    signingCredentials: signingCredentials
                );

            return jwtSecurityToken;
        }

        public async Task<CustomizedAPIResponse<bool>> ResetUserPasswordAsync(ResetPasswordRequestDto resetPasswordRequestDto)
        {
            var user = await _userManager.FindByEmailAsync(resetPasswordRequestDto.Email);

            if (user is null)
            {
                throw new ApiErrorExceptions($"User with this {resetPasswordRequestDto.Email} is not found.");
            }

            string token = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(resetPasswordRequestDto.Token));

            var identityResult =  await _userManager.ResetPasswordAsync(user, token, resetPasswordRequestDto.NewPassword);

            if (identityResult.Succeeded)
            {
                return new CustomizedAPIResponse<bool>(true, "Reset password successfully.");
            }
            else
            {
                throw new ApiErrorExceptions(identityResult.Errors.ToString()!);
            }
        }

        public async Task<CustomizedAPIResponse<bool>> ForgotPasswordAsync(string userEmail)
        {
            var user = await _userManager.FindByEmailAsync(userEmail);

            if (user is null)
            {
                throw new ApiErrorExceptions($"User with this {userEmail} is not found.");
            }

            string token = await _userManager.GeneratePasswordResetTokenAsync(user);

            byte[] encodedTokenArray = Encoding.UTF8.GetBytes(token);
            token = WebEncoders.Base64UrlEncode(encodedTokenArray);

            var resetPasswordLink = $"{_configuration["ClientUrl"]}/api/accounts/reset-password?email={userEmail}&token={token}";

            var emailRequest = new EmailRequestDto()
            {
                To = userEmail,
                Body = $"<p>click this link to reset your password: <a href='{resetPasswordLink}'>Reset Password</a></p>",
                Subject = $"Reset your password.",
                IsHtmlBody = true,
            };

            await _emailService.SendAsync(emailRequest);

            return new CustomizedAPIResponse<bool>(true, "Reset password link sent to your email account.");
        }

    }
}



//var emailRequest = new EmailRequestDto()
//{
//    To = newUser.Email,
//    Subject = $"Welcome {newUser.Email} to Clean Architecture Tutorial.",
//    Body = $"Hello {newUser.FirstName}, Welcome Clean Architecture Tutorial",
//    IsHtmlBody = false,
//};

//emailRequest.Body = $"User {newUser.FirstName} has Registered Successfully.";

//await _emailService.SendAsync(emailRequest);