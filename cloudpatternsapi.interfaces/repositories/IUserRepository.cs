using cloudpatternsapi.models;
using PagedListForEFCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace cloudpatternsapi.interfaces
{
    public interface IUserRepository
    {
        void Update(AppUser user);
        Task<PagedList<AppUser>> GetUsersAsync(UserParams userParams);
        Task<AppUser?> GetUserByIdAsync(int id);
        Task<AppUser?> GetUserByEmailAsync(string? email);
        Task<AppUser?> GetUserByUsernameAsync(string? username);
        Task<string> HealthCheck();
    }
}
