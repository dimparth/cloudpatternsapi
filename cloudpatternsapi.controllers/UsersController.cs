using AutoMapper;
using cloudpatternsapi.dto;
using cloudpatternsapi.implementation;
using cloudpatternsapi.interfaces;
using cloudpatternsapi.models;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace cloudpatternsapi.controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;

        public UsersController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpGet]
        public async Task<ActionResult<IList<UserDto>>> GetUsers([FromQuery] UserParams userParams)
        {
            var response = await _userService.GetUsers(userParams);
            Response.AddPaginationHeader(response.Item2.CurrentPage, response.Item2.PageSize, response.Item2.TotalCount, response.Item2.TotalPages);
            return Ok(response.Item1);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<IList<UserDto>>> GetUserById(string id)
        {
            return Ok(await _userService.GetById(id));
        }

        [HttpGet("GetByUsername/{username}")]
        public async Task<ActionResult<IList<UserDto>>> GetUserByUsername(string username)
        {
            return Ok(await _userService.GetByUsername(username));
        }


        //[HttpGet("GetByEmail/{email}")]
        //public async Task<ActionResult<IEnumerable<UserDto>>> GetUserByEmail(string email)
        //{
        //    var user = await _userRepository.GetUserByEmailAsync(email);
        //    if (user is null) return Ok(null);
        //    return Ok(_mapper.Map<UserDto>(user));
        //}
    }
}
