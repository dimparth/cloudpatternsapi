using AutoMapper;
using cloudpatternsapi.dto;
using cloudpatternsapi.interfaces;
using cloudpatternsapi.interfaces.services;
using cloudpatternsapi.models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Polly;
using Polly.CircuitBreaker;
using Polly.Retry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace cloudpatternsapi.implementation.services
{
    public class AccountService : IAccountService
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly ITokenService _tokenService;
        private readonly IMapper _mapper;
        private readonly AsyncRetryPolicy _retryPolicy;
        private readonly AsyncCircuitBreakerPolicy _circuitBreakerPolicy;

        public AccountService(UserManager<AppUser> userManager, SignInManager<AppUser> signInManager, ITokenService tokenService, IMapper mapper)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _tokenService = tokenService;
            _mapper = mapper;
            _retryPolicy = Policy.Handle<Exception>().RetryForeverAsync();
            _circuitBreakerPolicy = Policy.Handle<Exception>(result => string.IsNullOrEmpty(result.Message)).CircuitBreakerAsync(2, TimeSpan.FromSeconds(1));
        }

        public async Task<UserDto> RegisterNewUser(RegisterDto registerDto)
        {
            if (await _userManager.Users.AnyAsync(x => x.UserName == registerDto.Username!.ToLower())) throw new Exception();// return BadRequest("Το όνομα χρήστη " + registerDto.Username + " χρησιμοποιείται");

            var user = _mapper.Map<AppUser>(registerDto);

            user.UserName = registerDto.Username?.ToLower();

            var result = await _userManager.CreateAsync(user, registerDto.Password);

            if (!result.Succeeded) throw new Exception();//return BadRequest(result.Errors);

            var roleResult = await _userManager.AddToRoleAsync(user, "Member");

            if (!roleResult.Succeeded) throw new Exception($"error {roleResult.Errors}");// return BadRequest(roleResult.Errors);

            return new UserDto
            {
                Username = registerDto.Username,
                Token = await _tokenService.CreateToken(user),
                Gender = registerDto.Gender
            };
        }
        public async Task<UserDto> UserLogin(LoginDto loginDto)
        {
            var user = await _userManager.Users
                .SingleOrDefaultAsync(x => x.UserName == loginDto!.UserName!.ToLower());
            if (user == null) throw new Exception();// return BadRequest("Invalid Username");

            var result = await _signInManager
                .CheckPasswordSignInAsync(user, loginDto.Password, false);

            if (!result.Succeeded) throw new Exception();// return BadRequest("Λάθος κωδικός πρόσβασης");

            return new UserDto
            {
                Username = user.UserName,
                Token = await _tokenService.CreateToken(user),
                FirstName = user.FirstName,
                LastName = user.LastName,
                Gender = user.Gender,
                IsDisabled = user.IsDisabled
            };
        }
        public async Task<UserDto> FacebookLogin(ExternalAuthDto externalAuthDto)
        {
            var user = await _userManager.FindByEmailAsync(externalAuthDto.Email);
            if (user == null)
            {
                var appUser = new AppUser
                {
                    UserName = externalAuthDto.Email,
                    FirstName = externalAuthDto.FirstName,
                    LastName = externalAuthDto.LastName,
                    Email = externalAuthDto.Email
                };

                var result = await _userManager.CreateAsync(appUser);

                if (!result.Succeeded) throw new Exception();//return Unauthorized();

                var roleResult = await _userManager.AddToRoleAsync(appUser, "Member");

                if (!roleResult.Succeeded) throw new Exception();// return BadRequest(roleResult.Errors);

                return new UserDto
                {
                    Username = appUser.UserName,
                    Token = await _tokenService.CreateToken(appUser)
                };
            }
            else
            {
                return new UserDto
                {
                    Username = user.UserName,
                    Token = await _tokenService.CreateToken(user),
                    FirstName = user.FirstName ?? throw new Exception(),
                    LastName = user.LastName ?? throw new Exception(),
                    Gender = user.Gender,
                    IsDisabled = user.IsDisabled
                };
            }
        }
    }
}

