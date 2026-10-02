using LMS.Shared.DTOs.AuthDtos;

namespace Service.Contracts;

public interface IAuthService
{
    Task<TokenDto?> AuthenticateAsync(UserAuthDto userDto);
    Task<TokenDto> RefreshTokenAsync(TokenDto token);
}
