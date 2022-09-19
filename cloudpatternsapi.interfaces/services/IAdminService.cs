using cloudpatternsapi.dto.responses;
using cloudpatternsapi.models;
using PagedListForEFCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace cloudpatternsapi.interfaces.services
{
    public interface IAdminService
    {
        Task<Tuple<IList<UserRoleDto>, PagedListHeaders>> GetUsersAndRoles(UserParams userParams);
        Task<bool> ChangeUserStatus(string username);
        Task<IList<string>> EditRoles(string username, string roles);
    }
}
