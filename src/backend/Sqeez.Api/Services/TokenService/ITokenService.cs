using Sqeez.Api.Models.Users;

namespace Sqeez.Api.Services.TokenService
{
    public interface ITokenService
    {
        string CreateToken(Student user);
    }
}