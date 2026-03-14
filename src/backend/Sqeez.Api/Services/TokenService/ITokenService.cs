using Sqeez.Api.DTOs;
using Sqeez.Api.Models.Users;

namespace Sqeez.Api.Services.TokenService
{
    public interface ITokenService
    {
        ServiceResult<string> CreateToken(Student user);
        string GenerateRefreshToken();
    }
}