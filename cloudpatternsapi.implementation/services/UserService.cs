using AutoMapper;
using cloudpatternsapi.dto;
using cloudpatternsapi.interfaces;
using cloudpatternsapi.models;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using PagedListForEFCore;
using Polly;
using Polly.CircuitBreaker;
using Polly.Contrib.WaitAndRetry;
using Polly.Retry;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace cloudpatternsapi.implementation
{

    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<UserService> _logger;
        private static readonly AsyncRetryPolicy RetryPolicy = Policy.Handle<ArgumentException>().Or<SqlException>()
            .WaitAndRetryAsync(Backoff.DecorrelatedJitterBackoffV2(TimeSpan.FromSeconds(1), 3), onRetry: (exception, delay, context) =>
            {
                Debug.WriteLine($"{"Retry",-10}{delay,-10:ss\\.fff}: {exception.GetType().Name}");
            });
        private static readonly AsyncCircuitBreakerPolicy CircuitBreakerPolicy = Policy.Handle<ArgumentException>().Or<SqlException>().
            CircuitBreakerAsync(1, TimeSpan.FromMinutes(1),
            onBreak: (ex, @break) => Debug.WriteLine($"{"Break",-10}{@break,-10:ss\\.fff}: {ex.GetType().Name}"),
            onReset: () => Debug.WriteLine($"{"Reset",-10}"),
            onHalfOpen: () => Debug.WriteLine($"{"HalfOpen",-10}"));
        public UserService(IUserRepository userRepository, IMapper mapper, ILogger<UserService> logger)
        {
            _userRepository = userRepository;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<IList<UserDto>> GetById(string id)
        {
            if (CircuitBreakerPolicy.CircuitState == CircuitState.Open)
            {
                throw new Exception("Service unavailable");
            }
            var response = await CircuitBreakerPolicy.ExecuteAsync(() => RetryPolicy.ExecuteAsync(async () =>
            {
                var state = CircuitBreakerPolicy.CircuitState;

                var user = await _userRepository.GetUserByIdAsync(int.Parse(id));
                if (user == null) throw new ArgumentException("not found");
                var userList = _mapper.Map<List<UserDto>>(user);
                return userList;
            }));
            _logger.LogInformation(JsonSerializer.Serialize(response));
            return response;
        }

        public async Task<IList<UserDto>> GetByUsername(string username)
        {
            var user = await _userRepository.GetUserByUsernameAsync(username);
            if (user == null) throw new ArgumentException("not found");
            var response = _mapper.Map<List<UserDto>>(user);
            _logger.LogInformation(JsonSerializer.Serialize(response));
            return response;
        }

        public async Task<Tuple<IList<UserDto>, PagedListHeaders>> GetUsers(UserParams userParams)
        {
            if (CircuitBreakerPolicy.CircuitState == CircuitState.Open)
            {
                throw new Exception("Service unavailable");
            }
            PagedListHeaders header = new();
            var response = await CircuitBreakerPolicy.ExecuteAsync(() => RetryPolicy.ExecuteAsync(async () =>
            {
                Extensions.IsTransientError();
                var users = await _userRepository.GetUsersAsync(userParams);
                header = PagedList<AppUser>.ToHeader(users);
                var usersList = _mapper.Map<List<UserDto>>(users);
                return new Tuple<IList<UserDto>, PagedListHeaders>(usersList, header);
            }));
            _logger.LogInformation(JsonSerializer.Serialize(response.Item1));
            return response;
        }
    }
}
