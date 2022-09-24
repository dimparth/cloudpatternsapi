using AutoMapper;
using cloudpatternsapi.dto;
using cloudpatternsapi.interfaces;
using cloudpatternsapi.interfaces.services;
using cloudpatternsapi.models;
using PagedListForEFCore;
using Polly;
using Polly.CircuitBreaker;
using Polly.Contrib.WaitAndRetry;
using Polly.Retry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace cloudpatternsapi.implementation.services
{
    public class HallsService : IHallsService
    {
        private readonly IHallRepository _hallsRepository;
        private readonly IMapper _mapper;
        private readonly AsyncRetryPolicy _retryPolicy;
        private readonly AsyncCircuitBreakerPolicy _circuitBreakerPolicy;

        public HallsService(IHallRepository hallsRepository, IMapper mapper)
        {
            _hallsRepository = hallsRepository;
            _mapper = mapper;
            _retryPolicy = Policy.Handle<Exception>().WaitAndRetryAsync(Backoff.DecorrelatedJitterBackoffV2(TimeSpan.FromSeconds(1), 3));
            _circuitBreakerPolicy = Policy.Handle<Exception>(result => string.IsNullOrEmpty(result.Message)).CircuitBreakerAsync(2, TimeSpan.FromSeconds(1));
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

            if (await _hallsRepository.Complete()) Console.WriteLine("Ok");
            Console.WriteLine("NotOK");
        }

        public async Task<HallDto> GetHallById(int id)
        {
            var response = await _circuitBreakerPolicy.ExecuteAsync(() => _retryPolicy.ExecuteAsync(async () =>
            {
                var hall = await _hallsRepository.GetHallByIdAsync(id);

                if (hall == null) throw new Exception($"The hall with id {id} was not found");// return NotFound("The hall with id " + id + " was not found");

                return _mapper.Map<HallDto>(hall);
            }));
            return response;
            /*var hall = await _hallsRepository.GetHallByIdAsync(id);

            if (hall == null) throw new Exception();// return NotFound("The hall with id " + id + " was not found");

            return _mapper.Map<HallDto>(hall);*/
        }

        public async Task<Tuple<IList<HallDto>,PagedListHeaders>> GetHalls(HallParams hallParams)
        {
            var response = await _circuitBreakerPolicy.ExecuteAsync(() => _retryPolicy.ExecuteAsync(async () =>
            {
                var halls = await _hallsRepository.GetAllHallsAsync(hallParams);
                var header = PagedList<Hall>.ToHeader(halls);
                return new Tuple<IList<HallDto>, PagedListHeaders>(_mapper.Map<IList<HallDto>>(halls), header);
            }));
            return response;
            /*var halls = await _hallsRepository.GetAllHallsAsync(hallParams);
            var header = PagedList<Hall>.ToHeader(halls);
            return new Tuple<IList<HallDto>, PagedListHeaders>(_mapper.Map<IList<HallDto>>(halls), header);
            Response.AddPaginationHeader(halls.CurrentPage, halls.PageSize, halls.TotalCount, halls.TotalPages);

            return _mapper.Map<IEnumerable<HallDto>>(halls);*/
        }

        public async Task<IList<ShowDto>> GetShowsOfHall(int id, HallParams hallParams)
        {
            var response = await _circuitBreakerPolicy.ExecuteAsync(() => _retryPolicy.ExecuteAsync(async () =>
            {
                var shows = await _hallsRepository.GetShowsOfHallAsync(id, hallParams);

                if (shows == null) throw new Exception($"We could not found any show for hall {id}");

                return _mapper.Map<IList<ShowDto>>(shows);
            }));
            return response;
            /*var shows = await _hallsRepository.GetShowsOfHallAsync(id, hallParams);

            if (shows == null) throw new Exception();//return NotFound("We could not found any show for hall " + id);

            return _mapper.Map<IList<ShowDto>>(shows);*/
        }

        public async Task<IList<HallDto>> GetWithoutPagination()
        {
            var response = await _circuitBreakerPolicy.ExecuteAsync(() => _retryPolicy.ExecuteAsync(async () =>
            {
                var halls = await _hallsRepository.GetHallsWithoutPaginationAsync();

                return _mapper.Map<IList<HallDto>>(halls);
            }));
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
