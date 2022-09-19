using cloudpatternsapi.implementation;
using cloudpatternsapi.interfaces.services;
using cloudpatternsapi.models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace cloudpatternsapi.controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class AdminController : ControllerBase
	{
		private readonly IAdminService _adminService;

		public AdminController(IAdminService adminService)
		{
			_adminService = adminService;
		}

		[Authorize(Policy = "RequireAdminRole")]
		[HttpGet("users-with-roles")]
		public async Task<ActionResult> GetUsersWithRoles([FromQuery] UserParams userParams)
		{
			var response = await _adminService.GetUsersAndRoles(userParams);
			Response.AddPaginationHeader(response.Item2.CurrentPage, response.Item2.PageSize, response.Item2.TotalCount, response.Item2.TotalPages);
			return Ok(response.Item1);
		}

		[Authorize(Policy = "RequireAdminRole")]
		[HttpPost("change-status/{username}")]
		public async Task<ActionResult> ChangeUserStatus(string username)
		{
			var response = await _adminService.ChangeUserStatus(username);
			return Ok(response);
		}

		[HttpPost("edit-roles/{username}")]
		public async Task<ActionResult> EditRoles(string username, [FromQuery] string roles)
		{
			var response = await _adminService.EditRoles(username, roles);
			return Ok(response);
		}
	}
}
