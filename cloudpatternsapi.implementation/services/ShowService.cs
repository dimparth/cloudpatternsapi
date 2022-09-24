using AutoMapper;
using cloudpatternsapi.dto;
using cloudpatternsapi.interfaces;
using cloudpatternsapi.interfaces.services;
using cloudpatternsapi.models;
using Microsoft.Extensions.Caching.Memory;
using PagedListForEFCore;
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
    public class ShowService : IShowService
    {
        private readonly IShowRepository _showRepository;
        private readonly IHallRepository _hallRepository;
        private readonly IBookingRepository _bookingRepository;
        private readonly IMapper _mapper;
        private readonly AsyncRetryPolicy _retryPolicy;
        private readonly AsyncCircuitBreakerPolicy _circuitBreakerPolicy;
        private readonly IMemoryCache _memoryCache;
        private readonly ICacheService _cacheService;

        public ShowService(IShowRepository showRepository, IHallRepository hallRepository, IBookingRepository bookingRepository, IMapper mapper, IMemoryCache memoryCache, ICacheService cacheService)
        {
            _showRepository = showRepository;
            _hallRepository = hallRepository;
            _bookingRepository = bookingRepository;
            _mapper = mapper;
            _retryPolicy = Policy.Handle<Exception>().WaitAndRetryAsync(Backoff.DecorrelatedJitterBackoffV2(TimeSpan.FromSeconds(1), 3));
            _circuitBreakerPolicy = Policy.Handle<Exception>(result => string.IsNullOrEmpty(result.Message)).CircuitBreakerAsync(2, TimeSpan.FromSeconds(1));
            _memoryCache = memoryCache;
            _cacheService = cacheService;
        }

        public async Task<ShowDto> Add(CreateShowDto createShowDto)
        {
            if (string.IsNullOrEmpty(createShowDto.HallId.ToString())) throw new Exception("Invalid HallID");
            if (string.IsNullOrEmpty(createShowDto.Title)) throw new Exception("Show title cannot be empty");

            var response = await _circuitBreakerPolicy.ExecuteAsync(() => _retryPolicy.ExecuteAsync(async () =>
            {
                var hall = await _hallRepository.GetHallByIdAsync(createShowDto.HallId);

                if (hall == null) throw new Exception($"The hall with id {createShowDto.HallId} was not found");
                var show = _mapper.Map<Show>(createShowDto);

                _hallRepository.AddShow(hall, show);
                show.Hall = hall;
                _showRepository.Add(show);
                var isComplete = await _showRepository.Complete();
                if (!isComplete) throw new Exception($"Failed to add the show {createShowDto.Title}");
                var showToAdd = _mapper.Map<ShowDto>(show);
                return showToAdd;
            }));
            return response;
        }

        public async Task ChangeHallOfShow(int showId, int newHallId)
        {
            await _circuitBreakerPolicy.ExecuteAsync(() => _retryPolicy.ExecuteAsync(async () =>
            {
                var hall = await _hallRepository.GetHallByIdAsync(newHallId);

                if (hall is null) throw new Exception($"Could not find hall with id {newHallId}");

                var show = await _showRepository.GetShowByIdAsync(showId);

                if (show is null) throw new Exception($"Could not find show with id {showId}");

                show.Hall = hall;
                var isComplete = await _showRepository.Complete();
                if (!isComplete) throw new Exception($"Failed to change the hall of show {show.Title} to {hall.Title}.");
            }));
        }

        public async Task DeleteShow(int id)
        {
            var entityTodelete = await _showRepository.GetShowByIdAsync(id);

            if (entityTodelete is null) throw new Exception($"Could not find show with id {id}");

            _showRepository.Delete(entityTodelete);
            var isComplete = await _showRepository.Complete();
            if (!isComplete) throw new Exception($"Failed to delete the show {entityTodelete.Title}");
        }

        public async Task<ShowHallDto> GetHallOfShow(int id)
        {
            var response = await _circuitBreakerPolicy.ExecuteAsync(() => _retryPolicy.ExecuteAsync(async () =>
            {
                var hall = await _showRepository.GetHallOfShowAsync(id);

                if (hall == null) throw new Exception($"Could not find a hall for show with id {id}");
                var showToReturn = _mapper.Map<ShowHallDto>(hall);
                return showToReturn;
            }));
            return response;
        }

        public async Task<IList<SeatsShowDto>> GetSeatsOfShow(int showId, DateTime showDate)
        {
            var response = await _circuitBreakerPolicy.ExecuteAsync(() => _retryPolicy.ExecuteAsync(async () =>
            {
                var reservedSeats = (await _bookingRepository.GetReservedSeatsForShow(showId, showDate)).ToArray();

                var hall = await _showRepository.GetHallOfShowAsync(showId);

                var array = Extensions.CreateArrayOfSeats(reservedSeats, hall.Capacity);
                return array;
            }));
            return response;
        }

        public async Task<ShowDto> GetShowById(int id)
        {
            var response = await _circuitBreakerPolicy.ExecuteAsync(() => _retryPolicy.ExecuteAsync(async () =>
            {
                var show = await _showRepository.GetShowByIdAsync(id);

                if (show is null) throw new Exception($"The show with id {id} was not found");

                var toReturn = _mapper.Map<ShowDto>(show);
                return toReturn;
            }));
            return response;
        }

        public async Task<Tuple<IList<ShowDto>, PagedListHeaders>> GetShows(ShowParams showParams)
        {
            PagedListHeaders header = new();
            var response = await _circuitBreakerPolicy.ExecuteAsync(() => _retryPolicy.ExecuteAsync(async () =>
            {
                var cacheKey = new ShowCacheKey(showParams);
                var showsList = _cacheService.Get(cacheKey);
                if (showsList is null)
                {
                    var shows = await _showRepository.GetAllShowsAsync(showParams);
                    header = PagedList<Show>.ToHeader(shows);
                    showsList = _mapper.Map<IList<ShowDto>>(shows);
                    _cacheService.Add(showsList, cacheKey);
                }
                var ret = new Tuple<IList<ShowDto>, PagedListHeaders>(showsList, header);
                return ret;
            }));
            return response;
        }

        public async Task<IList<ShowDto>> GetShowsForDate(DateTime dateGiven)
        {
            var response = await _circuitBreakerPolicy.ExecuteAsync(() => _retryPolicy.ExecuteAsync(async () =>
            {
                var shows = await _showRepository.GetShowsForSpecificDateAsync(dateGiven);

                return _mapper.Map<IList<ShowDto>>(shows);
            }));
            return response;
        }

        public async Task UpdateShow(ShowUpdateDto showUpdateDto, int id)
        {
            var entityTochange = await _showRepository.GetShowByIdAsync(id);

            _mapper.Map(showUpdateDto, entityTochange);

            _showRepository.Update(entityTochange);
            var isComplete = await _showRepository.Complete();
            if (!isComplete) throw new Exception($"Failed to update the show {showUpdateDto.Title}");
        }
    }
}
