using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace cloudpatternsapi.dto
{
    public class HallDto
    {
        public int Id { get; init; }
        public string? Title { get; set; }
        public string? Description { get; set; }
        public string? Address { get; set; }
        public int Capacity { get; set; }
        public string? Phone { get; set; }
        public string? EmailAddress { get; set; }
        public ICollection<ShowDto>? Shows { get; set; }
    }
}
