using cloudpatternsapi.interfaces;
using cloudpatternsapi.interfaces.services;
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
    public class HealthcheckService : IHealthCheckService
    {
        private readonly IUserRepository _userRepository;
        private readonly AsyncRetryPolicy _retryPolicy;
        private readonly AsyncCircuitBreakerPolicy _circuitBreakerPolicy;

        public HealthcheckService(IUserRepository userRepository)
        {
            _userRepository = userRepository;
            _retryPolicy = Policy.Handle<Exception>().RetryForeverAsync();
            _circuitBreakerPolicy = Policy.Handle<Exception>(result => string.IsNullOrEmpty(result.Message)).CircuitBreakerAsync(2, TimeSpan.FromSeconds(1));
        }

        public async Task<string> ApiHealthCheck()
        {
            var result = await Task.Run(() => {
                return DateTime.Now.ToString();
            });
            return result;
        }

        public async Task<string> DatabaseHealthCheck()
        {
            var result = await _userRepository.HealthCheck();
            return result;
        }
        public async Task<string> DiskSizeCheck()
        {
            DriveInfo[] allDrives = DriveInfo.GetDrives();
            var stringBuilder = new StringBuilder();
            var result = await Task.Run(() => {
                foreach (DriveInfo drive in allDrives)
                {
                    stringBuilder.Append($"Drive {drive.Name}. ");
                    stringBuilder.Append($"Drive type:  {drive.DriveType}. ");
                    if (drive.IsReady == true)
                    {
                        stringBuilder.Append($"Available space: {drive.AvailableFreeSpace} bytes. ");
                        stringBuilder.Append($"Total available space: {drive.TotalFreeSpace} bytes. ");
                        stringBuilder.Append($"Total size of drive: {drive.TotalSize} bytes. ");
                    }
                }
                return stringBuilder.ToString();
            });
            return result.ToString();
        }
    }
}
