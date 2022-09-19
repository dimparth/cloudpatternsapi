using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace cloudpatternsapi.models
{
    public class UserParams : PaginationParams
    {
        public string? SearchUsername { get; set; }
        public string? OrderBy { get; set; }
    }
}
