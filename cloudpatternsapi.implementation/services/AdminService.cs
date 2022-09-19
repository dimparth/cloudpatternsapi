using cloudpatternsapi.dto.responses;
using cloudpatternsapi.interfaces.services;
using cloudpatternsapi.models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using PagedListForEFCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace cloudpatternsapi.implementation.services
{
    public class AdminService : IAdminService
    {

        private readonly UserManager<AppUser> _userManager;

        public AdminService(UserManager<AppUser> userManager)
        {
            _userManager = userManager;
        }

        public async Task<Tuple<IList<UserRoleDto>,PagedListHeaders>> GetUsersAndRoles(UserParams userParams)
        {
			var query = _userManager.Users.Include(r => r.UserRoles)!
				.ThenInclude(r => r.Role)
				.OrderBy(u => u.UserName)
				.Where(u => u.UserName != "admin")
				.AsQueryable();

			if (!string.IsNullOrWhiteSpace(userParams.SearchUsername))
			{
				query = query.Where(x => x.UserName.Contains(userParams.SearchUsername));
			}

			query = userParams.OrderBy switch
			{
				"username" => query.OrderByDescending(s => s.UserName),
				_ => query.OrderByDescending(s => s.CreationDate)
			};

			var users = await PagedList<AppUser>.CreateAsync(query, userParams.PageNumber, userParams.PageSize);
			var headers = PagedList<AppUser>.ToHeader(users);

			var newData = users.Select(u => new UserRoleDto
			{
				Id = u.Id,
				UserName = u.UserName,
				FirstName = u.FirstName,
				LastName = u.LastName,
				Email = u.Email,
				DateOfBirth = u.DateOfBirth.ToString("dd/MM/yyyy"),
				CreationDate = u.CreationDate.ToString("dd/MM/yyyy HH:mm:ss"),
				IsDisabled = u.IsDisabled,
				Roles = u.UserRoles!.Select(r => r.Role!.Name).ToList()
			});
			return new Tuple<IList<UserRoleDto>, PagedListHeaders>(newData.ToList(), headers);
		}
		public async Task<bool> ChangeUserStatus(string username)
        {
			var user = await _userManager.FindByNameAsync(username);

			if (user == null) throw new Exception();//return NotFound("Δεν ήταν δυνατή η εύρεση του χρήστη");

			user.IsDisabled = !user.IsDisabled;

			var result = await _userManager.UpdateAsync(user);

			if (!result.Succeeded) throw new Exception(); //return BadRequest("Δεν ήταν δυνατή η αλλαγή του στατούς του χρήστη");

			return user.IsDisabled;
		}

		public async Task<IList<string>> EditRoles(string username, string roles)
        {
			var selectedRoles = roles.Split(",").ToArray();

			var user = await _userManager.FindByNameAsync(username);

			if (user == null) throw new Exception();// return NotFound("Δεν ήταν δυνατή η εύρεση του χρήστη");

			var userRoles = await _userManager.GetRolesAsync(user);

			var result = await _userManager.AddToRolesAsync(user, selectedRoles.Except(userRoles));

			if (!result.Succeeded) throw new Exception();// return BadRequest("Δεν ήταν δυνατή η προσθήκη του ρόλου");

			result = await _userManager.RemoveFromRolesAsync(user, userRoles.Except(selectedRoles));

			if (!result.Succeeded) throw new Exception();// return BadRequest("Δεν ήταν δυνατή η αφαίρεση του ρόλου");

			return await _userManager.GetRolesAsync(user);
		}

	}
}
