using ServiceUser.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace ServiceUser
{
    public interface IUserService
    {
        
        Task<UserReadDTO?> GetUserByIdAsync(int id);
        

        
        Task<UserReadDTO> AddUserAsync(UserCreateDto dto);

        
        Task<UserReadDTO> UpdateUserAsync(int id, UserUpdateDto dto);

        
        Task<UserReadDTO> DeleteUserAsync(int id);

        Task<UserReadDTO?> AuthenticateAsync(string email, string password);

        Task<UserReadDTO> ResetPasswordAsync(string email, string novaSenha);
        Task<UserInternalDTO> GetUserByIdApiAsync(int id);
    }
}
