using cloudpatternsapi.dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace cloudpatternsapi.interfaces.services
{
    public interface IAccountService
    {
        Task<UserDto> RegisterNewUser(RegisterDto registerDto);
        Task<UserDto> UserLogin(LoginDto loginDto);
        Task<UserDto> FacebookLogin(ExternalAuthDto externalAuthDto);
    }
}
