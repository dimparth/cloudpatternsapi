using AutoMapper;
using cloudpatternsapi.dto;
using cloudpatternsapi.interfaces;
using cloudpatternsapi.interfaces.services;
using cloudpatternsapi.models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.CircuitBreaker;
using Polly.Retry;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace cloudpatternsapi.implementation.services
{
    public class AccountService : IAccountService
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly ITokenService _tokenService;
        private readonly IMapper _mapper;
        private readonly ILogger<AccountService> _logger;
        private static readonly AsyncCircuitBreakerPolicy CircuitBreakerPolicy = Policy.Handle<ArgumentException>().CircuitBreakerAsync(2, TimeSpan.FromMinutes(1),
            onBreak: (ex, @break) => Debug.WriteLine($"{"Break",-10}{@break,-10:ss\\.fff}: {ex.GetType().Name}"),
            onReset: () => Debug.WriteLine($"{"Reset",-10}"),
            onHalfOpen: () => Debug.WriteLine($"{"HalfOpen",-10}"));

        public AccountService(UserManager<AppUser> userManager, SignInManager<AppUser> signInManager, ITokenService tokenService, IMapper mapper, ILogger<AccountService> logger)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _tokenService = tokenService;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<UserDto> RegisterNewUser(RegisterDto registerDto)
        {
            if (await _userManager.Users.AnyAsync(x => x.UserName == registerDto.Username!.ToLower())) throw new Exception($"Το όνομα χρήστη {registerDto.Username} χρησιμοποιείται");
            var user = _mapper.Map<AppUser>(registerDto);

            user.UserName = registerDto.Username?.ToLower();

            var result = await _userManager.CreateAsync(user, registerDto.Password);

            if (!result.Succeeded) throw new ArgumentException($"{result.Errors}");

            var roleResult = await _userManager.AddToRoleAsync(user, "Member");

            if (!roleResult.Succeeded) throw new ArgumentException($"error {roleResult.Errors}");

            var response =  new UserDto
            {
                Username = registerDto.Username,
                Token = await _tokenService.CreateToken(user),
                Gender = registerDto.Gender
            };
            _logger.LogInformation(JsonSerializer.Serialize(response));
            return response;
        }
        public async Task<UserDto> UserLogin(LoginDto loginDto)
        {
            if (CircuitBreakerPolicy.CircuitState == CircuitState.Open)
            {
                throw new Exception("Unable to login at this moment. Please try again later");
            }
            var response = await CircuitBreakerPolicy.ExecuteAsync(async () =>
            {
                if (!loginDto.UserName.ValidateUserInput()) throw new ArgumentException("Μη έγκυρο Username");
                var user = await _userManager.Users
                .SingleOrDefaultAsync(x => x.UserName == loginDto!.UserName!.ToLower());
                if (user == null) throw new ArgumentException("Το Username δεν βρέθηκε");

                var result = await _signInManager
                    .CheckPasswordSignInAsync(user, loginDto.Password, false);

                if (!result.Succeeded) throw new ArgumentException("Λάθος κωδικός πρόσβασης");

                return new UserDto
                {
                    Username = user.UserName,
                    Token = await _tokenService.CreateToken(user),
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Gender = user.Gender,
                    IsDisabled = user.IsDisabled
                };
            });
            _logger.LogInformation(JsonSerializer.Serialize(response));
            return response;


        }
        public async Task<UserDto> FacebookLogin(ExternalAuthDto externalAuthDto)
        {
            var user = await _userManager.FindByEmailAsync(externalAuthDto.Email);
            if (user is not null)
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
            //if (user == null)
            //{
            var appUser = new AppUser
            {
                UserName = externalAuthDto.Email,
                FirstName = externalAuthDto.FirstName,
                LastName = externalAuthDto.LastName,
                Email = externalAuthDto.Email
            };

            var result = await _userManager.CreateAsync(appUser);

            if (!result.Succeeded) throw new ArgumentException("Unauthorized user");

            var roleResult = await _userManager.AddToRoleAsync(appUser, "Member");

            if (!roleResult.Succeeded) throw new ArgumentException($"{roleResult.Errors}");

            return new UserDto
            {
                Username = appUser.UserName,
                Token = await _tokenService.CreateToken(appUser)
            };
            //}
            /*else
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
            }*/
        }
    }
}

