using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nabla.Mis
{
    public interface IUserModel
    {
        string UserName { get; }

        string Password { get; }

        ICollection<string> Roles { get; }
    }
}
