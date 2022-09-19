using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace cloudpatternsapi.models
{
    public class ShowParams : PaginationParams
    {
        public string? SearchTitle { get; set; }
        //public DateTime SearchDate { get; set; } = DateTime.Now;
        public string? OrderBy { get; set; }
    }
}
