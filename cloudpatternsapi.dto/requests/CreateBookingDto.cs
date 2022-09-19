using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace cloudpatternsapi.dto
{
    public class CreateBookingDto
    {
        public int ShowId { get; init; }
        public DateTime DateOfShow { get; set; }
        public string[]? Seats { get; set; }
    }
}
