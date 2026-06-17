using ServiceUser.DTOs;

namespace ServiceUser
{
    public interface IUserService
    {
        Task<UserReadDTO?> GetUserByIdAsync(int id);
        Task<UserInternalDTO?> GetUserByIdApiAsync(int id);

        Task<UserReadDTO> AddUserAsync(UserCreateDto dto);
        Task<UserReadDTO> UpdateUserAsync(int id, UserUpdateDto dto);
        Task<UserReadDTO> DeleteUserAsync(int id);

        Task<UserReadDTO?> AuthenticateAsync(string email, string password);
        Task<UserReadDTO> ResetPasswordAsync(string email, string novaSenha);
    }
}
