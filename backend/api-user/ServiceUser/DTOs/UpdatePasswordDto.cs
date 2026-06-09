using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceUser
{
    public class UpdatePasswordDto
    {
        public int UserId { get; set; }
        public string NewPassword { get; set; }
    }
}
