using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DominioDenuncia;

namespace ServiceDenuncia
{
    public interface IDenunciaService
    {
        Task<DenunciaDto> CreatDenunciaAsync(DenunciaCreateDto dto);
        Task<IEnumerable<DenunciaDto?>> GetAllDenunciasAsync(int id);
        Task<DenunciaDto?> GetDenunciaByIdAsync(int id);
        void UpdateByIdUser(int id);
    }
}
