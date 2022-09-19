using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace cloudpatternsapi.dto
{
    public class SeatsShowDto
    {
        public string? SeatNumber { get; set; }
        public bool IsAvailable { get; set; }
    }
}
