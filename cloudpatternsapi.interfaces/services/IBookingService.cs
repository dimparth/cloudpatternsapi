using cloudpatternsapi.dto;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace cloudpatternsapi.interfaces
{
    public interface IBookingService
    {
        Task<BookingDto> CreateBooking(CreateBookingDto createBookingDto, int userId);
        Task<IList<BookingDto>> GetBookingsForLoggedUser(int userId);
        Task<IList<BookingDto>> GetBookingsForShowAndDate(int showId, DateTime showDate);
        Task<IList<BookingDto>> GetBookingForUserByEmail(FileDto model);
        Task<bool> UpdateBookingToAppeared(int id);
        Task<bool> SaveFileAsync(IFormFile? file);
    }
}
