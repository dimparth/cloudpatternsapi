using AutoMapper;
using cloudpatternsapi.dto;
using cloudpatternsapi.interfaces;
using cloudpatternsapi.interfaces.services;
using cloudpatternsapi.models;
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

namespace cloudpatternsapi.implementation.services
{
    public class HallsService : IHallsService
    {
        private readonly IHallRepository _hallsRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<HallsService> _logger;
        private static readonly AsyncRetryPolicy RetryPolicy = Policy.Handle<ArgumentException>().Or<Microsoft.Data.SqlClient.SqlException>()
            .WaitAndRetryAsync(Backoff.DecorrelatedJitterBackoffV2(TimeSpan.FromSeconds(1), 3), onRetry: (exception, delay, context) =>
            {
                Debug.WriteLine($"{"Retry",-10}{delay,-10:ss\\.fff}: {exception.GetType().Name}");
            });
        private static readonly AsyncCircuitBreakerPolicy CircuitBreakerPolicy = Policy.Handle<ArgumentException>().CircuitBreakerAsync(1, TimeSpan.FromMinutes(1),
            onBreak: (ex, @break) => Debug.WriteLine($"{"Break",-10}{@break,-10:ss\\.fff}: {ex.GetType().Name}"),
            onReset: () => Debug.WriteLine($"{"Reset",-10}"),
            onHalfOpen: () => Debug.WriteLine($"{"HalfOpen",-10}"));

        public HallsService(IHallRepository hallsRepository, IMapper mapper, ILogger<HallsService> logger)
        {
            _hallsRepository = hallsRepository;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task AddHall(HallDto hallDto)
        {
            var hall = new Hall()
            {
                Title = hallDto.Title,
                Description = hallDto.Description,
                Address = hallDto.Address,
                Capacity = hallDto.Capacity,
                Phone = hallDto.Phone,
                EmailAddress = hallDto.EmailAddress
            };

            _hallsRepository.Add(hall);

            if (await _hallsRepository.Complete()) 
            {
                _logger.LogInformation($"Hall added {JsonSerializer.Serialize(hall)}");
            }
        }

        public async Task<HallDto> GetHallById(int id)
        {
            var response = await CircuitBreakerPolicy.ExecuteAsync(() => RetryPolicy.ExecuteAsync(async () =>
            {
                var hall = await _hallsRepository.GetHallByIdAsync(id);

                if (hall == null) throw new Exception($"The hall with id {id} was not found");

                return _mapper.Map<HallDto>(hall);
            }));
            _logger.LogInformation($"GetHallById response{JsonSerializer.Serialize(response)}");
            return response;
            /*var hall = await _hallsRepository.GetHallByIdAsync(id);

            if (hall == null) throw new Exception();// return NotFound("The hall with id " + id + " was not found");

            return _mapper.Map<HallDto>(hall);*/
        }

        public async Task<Tuple<IList<HallDto>,PagedListHeaders>> GetHalls(HallParams hallParams)
        {
            var response = await CircuitBreakerPolicy.ExecuteAsync(() => RetryPolicy.ExecuteAsync(async () =>
            {
                var halls = await _hallsRepository.GetAllHallsAsync(hallParams);
                var header = PagedList<Hall>.ToHeader(halls);
                return new Tuple<IList<HallDto>, PagedListHeaders>(_mapper.Map<IList<HallDto>>(halls), header);
            }));
            _logger.LogInformation($"GetHalls response{JsonSerializer.Serialize(response.Item1)}");
            return response;
            /*var halls = await _hallsRepository.GetAllHallsAsync(hallParams);
            var header = PagedList<Hall>.ToHeader(halls);
            return new Tuple<IList<HallDto>, PagedListHeaders>(_mapper.Map<IList<HallDto>>(halls), header);
            Response.AddPaginationHeader(halls.CurrentPage, halls.PageSize, halls.TotalCount, halls.TotalPages);

            return _mapper.Map<IEnumerable<HallDto>>(halls);*/
        }

        public async Task<IList<ShowDto>> GetShowsOfHall(int id, HallParams hallParams)
        {
            var response = await CircuitBreakerPolicy.ExecuteAsync(() => RetryPolicy.ExecuteAsync(async () =>
            {
                var shows = await _hallsRepository.GetShowsOfHallAsync(id, hallParams);

                if (shows == null) throw new Exception($"We could not found any show for hall {id}");

                return _mapper.Map<IList<ShowDto>>(shows);
            }));
            _logger.LogInformation($"GetShowsOfHall response{JsonSerializer.Serialize(response)}");
            return response;
            /*var shows = await _hallsRepository.GetShowsOfHallAsync(id, hallParams);

            if (shows == null) throw new Exception();//return NotFound("We could not found any show for hall " + id);

            return _mapper.Map<IList<ShowDto>>(shows);*/
        }

        public async Task<IList<HallDto>> GetWithoutPagination()
        {
            var response = await CircuitBreakerPolicy.ExecuteAsync(() => RetryPolicy.ExecuteAsync(async () =>
            {
                var halls = await _hallsRepository.GetHallsWithoutPaginationAsync();

                return _mapper.Map<IList<HallDto>>(halls);
            }));
            _logger.LogInformation($"GetWithoutPagination response{JsonSerializer.Serialize(response)}");
            return response;
            /*var halls = await _hallsRepository.GetHallsWithoutPaginationAsync();

            return _mapper.Map<IList<HallDto>>(halls);*/
        }

        public async Task UpdateHall(HallUpdateDto hallUpdateDto, int id)
        {
            var entityTochange = await _hallsRepository.GetHallByIdAsync(id);

            _mapper.Map(hallUpdateDto, entityTochange);

            _hallsRepository.Update(entityTochange);

            await _hallsRepository.Complete();
        }
    }
}
