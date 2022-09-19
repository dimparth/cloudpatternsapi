using AutoMapper;
using cloudpatternsapi.dto;
using cloudpatternsapi.interfaces;
using cloudpatternsapi.models;
using Microsoft.AspNetCore.Http;
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
    public class BookingService : IBookingService
    {
        private readonly IShowRepository _showRepository;
        private readonly IBookingRepository _bookingRepository;
        private readonly IUserRepository _userRepository;
        private readonly IEmailService _emailService;
        private readonly IMapper _mapper;
        private readonly string _appDirectory = Path.Combine(Directory.GetCurrentDirectory(), "Resources");
        private readonly AsyncRetryPolicy _retryPolicy;
        private readonly AsyncCircuitBreakerPolicy _circuitBreakerPolicy;

        public BookingService(IShowRepository showRepository, IBookingRepository bookingRepository, IUserRepository userRepository, IEmailService emailService, IMapper mapper)
        {
            _showRepository = showRepository;
            _bookingRepository = bookingRepository;
            _userRepository = userRepository;
            _emailService = emailService;
            _mapper = mapper;
            _retryPolicy = Policy.Handle<Exception>().RetryForeverAsync();
            _circuitBreakerPolicy = Policy.Handle<Exception>(result => string.IsNullOrEmpty(result.Message)).CircuitBreakerAsync(2, TimeSpan.FromSeconds(1));
        }
        public async Task<BookingDto> CreateBooking(CreateBookingDto createBookingDto, int userId)
        {
            var user = await _userRepository.GetUserByIdAsync(userId);

            var hall = await _showRepository.GetHallOfShowAsync(createBookingDto.ShowId);

            foreach (var seat in createBookingDto.Seats!)
            {
                if (!Extensions.IsValidSeat(hall.Capacity, seat))
                {
                    throw new Exception($"The selected seat {seat} has to be between 1 and {hall.Capacity}");
                }
            }

            bool checkIfReserved = await _bookingRepository.CheckIfReserved(createBookingDto.ShowId, createBookingDto.DateOfShow, user);
            if (checkIfReserved) throw new Exception("User has already made a reservation for this show");

            var reserved = (await _bookingRepository.GetReservedSeatsForShow(createBookingDto.ShowId, createBookingDto.DateOfShow)).ToArray();

            var taken = Extensions.CheckSeatsAvailability(createBookingDto.Seats, reserved);

            if (taken) throw new Exception("Some seats are already taken");

            int availbleSeats = hall.Capacity - reserved.ToArray().Length;

            if (Extensions.NumOfRequestedSeatsMoreThanAvailable(availbleSeats, createBookingDto.Seats.Length)) throw new Exception($"User has requested more seats( {createBookingDto.Seats.Length} ) than available( {availbleSeats} )");

            var show = await _showRepository.GetShowByIdAsync(createBookingDto.ShowId);
            if (show is null) throw new Exception($"The show with id {createBookingDto.ShowId} was not found");

            Booking booking = new()
            {
                Show = show,
                DateOfShow = createBookingDto.DateOfShow,
                User = user,
                NumOfSeats = createBookingDto.Seats.Length
            };
            _bookingRepository.CreateBooking(booking);
            if (!await _bookingRepository.Complete()) throw new Exception("Failed to add the booking ");

            _bookingRepository.ReserveSeatsForBooking(booking, createBookingDto.Seats, user);
            if (!await _bookingRepository.Complete()) throw new Exception();
            string dateOfShow = booking.DateOfShow.ToString();
            string timeOfShow = show.TimeStart.ToString();
            _emailService.SendEmail("Στοιχεία Κράτησης", user?.Email, user?.CreateEmailText(show.Title!, dateOfShow, timeOfShow, String.Join("-", booking.Seats!.Select(seat => seat.SeatNumber))));
            return _mapper.Map<BookingDto>(booking);
        }
        public async Task<IList<BookingDto>> GetBookingsForLoggedUser(int userId)
        {
            var response = await _circuitBreakerPolicy.ExecuteAsync(() => _retryPolicy.ExecuteAsync(async () =>
            {

                var user = await _userRepository.GetUserByIdAsync(userId);
                var bookings = await _bookingRepository.GetBookingsForUserAync(user);
                return _mapper.Map<List<BookingDto>>(bookings);
            }));
            return response;
            /*var user = await _userRepository.GetUserByIdAsync(userId);
            var bookings = await _bookingRepository.GetBookingsForUserAync(user);
            return _mapper.Map<List<BookingDto>>(bookings);*/
        }
        public async Task<IList<BookingDto>> GetBookingsForShowAndDate(int showId, DateTime showDate)
        {
            var response = await _circuitBreakerPolicy.ExecuteAsync(() => _retryPolicy.ExecuteAsync(async () =>
            {
                var bookings = await _bookingRepository.GetBookingsForShowAndDate(showId, showDate);

                return _mapper.Map<List<BookingDto>>(bookings);
            }));
            return response;
            /*var bookings = await _bookingRepository.GetBookingsForShowAndDate(showId, showDate);

            return _mapper.Map<List<BookingDto>>(bookings);*/
        }
        public async Task<IList<BookingDto>> GetBookingForUserByEmail(FileDto model)
        {
            var response = await _circuitBreakerPolicy.ExecuteAsync(() => _retryPolicy.ExecuteAsync(async () =>
            {
                var user = await _userRepository.GetUserByEmailAsync(model.EmailAddress);

                if (user == null) throw new Exception("User not found");
                var bookings = await _bookingRepository.GetBookingsForUserNotAppearedAync(user);
                var result = await SaveFileAsync(model.MyFile);
                return _mapper.Map<List<BookingDto>>(bookings);
            }));
            return response;
           /* var user = await _userRepository.GetUserByEmailAsync(model.EmailAddress);

            if (user == null) throw new Exception("User not found");
            var bookings = await _bookingRepository.GetBookingsForUserNotAppearedAync(user);
            var result = await SaveFileAsync(model.MyFile);
            return _mapper.Map<List<BookingDto>>(bookings);*/
        }
        public async Task<bool> UpdateBookingToAppeared(int id)
        {
            await _bookingRepository.SetAppearForBooking(id);
            var result = await _bookingRepository.Complete();
            return result;
        }
        public async Task<bool> SaveFileAsync(IFormFile? file)
        {
            if (file != null)
            {
                if (!Directory.Exists(_appDirectory))
                    Directory.CreateDirectory(_appDirectory);

                var fileName = DateTime.Now.Ticks.ToString() + Path.GetExtension(file.FileName);
                var path = Path.Combine(_appDirectory, fileName);

                using (var stream = new FileStream(path, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }
                return true;
            }
            return false;
        }
    }
}
