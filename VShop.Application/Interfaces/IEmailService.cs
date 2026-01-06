using VShop.Application.Dtos.Email;

namespace VShop.Application.Interfaces
{
    public interface IEmailService
    {
        Task SendAsync(EmailRequestDto emailRequestDto);

    }
}
