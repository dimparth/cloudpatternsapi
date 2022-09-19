using cloudpatternsapi.dto;
using cloudpatternsapi.models;
using PagedListForEFCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace cloudpatternsapi.interfaces
{
    public interface IUserService
    {
        public Task<Tuple<IList<UserDto>, PagedListHeaders>> GetUsers(UserParams userParams);
        public Task<IList<UserDto>> GetById(string id);
        public Task<IList<UserDto>> GetByUsername(string username);
    }
}
