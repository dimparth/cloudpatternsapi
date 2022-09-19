using cloudpatternsapi.dto;
using cloudpatternsapi.models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using System.Threading.Tasks;
using cloudpatternsapi.interfaces;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using cloudpatternsapi.interfaces.services;

namespace cloudpatternsapi.controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly IAccountService _accountService;

        public AccountController(IAccountService accountService)
        {
            _accountService = accountService;
        }

        [HttpPost("register")]
        public async Task<ActionResult<UserDto>> Register(RegisterDto registerDto)
        {
            var response = await _accountService.RegisterNewUser(registerDto);
            return Ok(response);
        }

        [HttpPost("login")]
        public async Task<ActionResult<UserDto>> Login(LoginDto loginDto)
        {
            var response = await _accountService.UserLogin(loginDto);
            return Ok(response);
        }

        [HttpPost("facebook-login")]
        public async Task<ActionResult<UserDto>> FacebookLogin(ExternalAuthDto externalAuthDto)
        {
            var response = await _accountService.FacebookLogin(externalAuthDto);
            return Ok(response);
        }
    }
}
