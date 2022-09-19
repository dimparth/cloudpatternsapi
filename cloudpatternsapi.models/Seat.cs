using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace cloudpatternsapi.models
{
    public class Seat
    {
        public int Id { get; init; }
        public Booking? Booking { get; init; }
        public string? SeatNumber { get; set; }
    }
}
