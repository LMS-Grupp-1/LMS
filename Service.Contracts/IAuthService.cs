using LMS.Shared.DTOs.AuthDtos;

namespace Service.Contracts;

public interface IAuthService
{
    Task<TokenDto> CreateTokenAsync(bool addTime);
    Task<TokenDto> RefreshTokenAsync(TokenDto token);
    Task<bool> ValidateUserAsync(UserAuthDto userDto);
}