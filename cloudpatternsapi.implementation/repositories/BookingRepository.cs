using cloudpatternsapi.interfaces;
using cloudpatternsapi.models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace cloudpatternsapi.implementation.repositories
{
    public class BookingRepository : IBookingRepository
    {
        private readonly BookingContext _context;

        public BookingRepository(BookingContext context)
        {
            _context = context;
        }

        public void CreateBooking(Booking booking)
        {
            _context.Bookings?.Add(booking);
            int id = booking.Id;
        }

        public async Task<IList<string>> GetReservedSeatsForShow(int showId, DateTime dateOfShow)
        {
            var seats = await _context.Seats!
                .Include(p => p.Booking)
                .Where(p => p.Booking!.Show!.Id == showId && p.Booking.DateOfShow == dateOfShow)
                .ToListAsync();

            return seats.Select(p => p.SeatNumber).ToList()!;
        }

        public void ReserveSeatsForBooking(Booking booking, string[] seats, AppUser? user)
        {
            foreach (string seatNum in seats)
            {
                Seat seat = new()
                {
                    Booking = booking,
                    SeatNumber = seatNum
                };
                _context.Seats?.Add(seat);
            }
        }

        public async Task<bool> CheckIfReserved(int showId, DateTime dateOfShow, AppUser? user)
        {
            return await _context.Bookings!
                .AnyAsync(p => p.Show!.Id == showId && p.DateOfShow == dateOfShow && p.User!.Id == user!.Id);
        }

        public async Task<bool> Complete()
        {
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<IReadOnlyList<Booking>> GetBookingsForUserAync(AppUser? user)
        {
            var data = await _context.Bookings!
                .Include(s => s.Show)
                .Include(s => s.Seats)
                .Where(p => p.User!.Id == user!.Id && p.Appeared == false)
                .ToListAsync();
            return data;
        }

        public async Task<IList<Booking>> GetBookingsForShowAndDate(int showId, DateTime dateGiven)
        {
            return await _context.Bookings!
                .Where(b => b.DateOfShow == dateGiven && b.Show!.Id == showId)
                .Include(b => b.Seats)
                .Include(b => b.User)
                .ToListAsync();
        }
        public async Task SetAppearForBooking(int bookingId)
        {
            var booking = await _context.Bookings!.Where(b => b.Id == bookingId).FirstOrDefaultAsync();
            booking!.Appeared = true;
        }

        public async Task<IList<Booking>> GetBookingsForUserNotAppearedAync(AppUser? user)
        {
            var data = await _context.Bookings!
                .Where(p => p.User!.Id == user!.Id && p.Appeared == false)
                .Include(s => s.Show)
                .Include(s => s.Seats)
                .ToListAsync();
            return data!;
        }
    }
}
