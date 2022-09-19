using cloudpatternsapi.models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace cloudpatternsapi.interfaces
{
    public interface ITokenService
    {
        Task<string> CreateToken(AppUser user);
    }
}
