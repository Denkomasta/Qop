namespace Sqeez.Api.Services.Interfaces
{
    public interface IEmailService
    {
        Task SendVerificationEmailAsync(string email, string verificationLink);
        Task SendPasswordResetEmailAsync(string email, string resetLink);
    }
}