using ServiceDenuncia.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceDenuncia
{
    public interface IUserApiClient
    {
        Task<UserBasicDto> UserExistsAsync(int userId);
    }
}
