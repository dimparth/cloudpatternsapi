using AutoMapper;
using cloudpatternsapi.dto;
using cloudpatternsapi.implementation;
using cloudpatternsapi.interfaces;
using cloudpatternsapi.models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace cloudpatternsapi.controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BookingController : ControllerBase
    {
        private readonly IBookingService _bookingService;
        public BookingController(IBookingService bookingService)
        {
            _bookingService = bookingService;
        }

        [Authorize]
        [HttpPost]
        public async Task<ActionResult> CreateBooking(CreateBookingDto createBookingDto)
        {
            var response = await _bookingService.CreateBooking(createBookingDto, User.GetUserId());
            return Ok(response);
        }

        [Authorize]
        [HttpGet("GetBookingsForLoggedUser")]
        public async Task<ActionResult<IEnumerable<BookingDto>>> GetBookingsForLoggedUser()
        {
            var response = await _bookingService.GetBookingsForLoggedUser(User.GetUserId());
            return Ok(response);
        }

        [Authorize]
        [HttpGet("GetBookingsForShowAndDate")]
        public async Task<ActionResult<IEnumerable<BookingDto>>> GetBookingsForShowAndDate([FromQuery] int showId, [FromQuery] DateTime date)
        {
            var response = await _bookingService.GetBookingsForShowAndDate(showId, date);
            return Ok(response);
        }

        [HttpPost("GetBookingForUserByEmail")]
        public async Task<ActionResult<BookingDto>> GetBookingForUserByEmail([FromForm] FileDto model)
        {
            var response = await _bookingService.GetBookingForUserByEmail(model);
            return Ok(response);
        }

        [HttpPatch("{id}")]
        public async Task UpdateBookingToAppeared(int id)
        {
            await _bookingService.UpdateBookingToAppeared(id);
        }
    }
}
