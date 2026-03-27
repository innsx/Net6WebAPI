using Application.DTOs;

namespace Application.Interfaces
{
    public interface IEmailService
    {
        Task SendAsync(EmailRequestDto emailRequestDto);
    }
}