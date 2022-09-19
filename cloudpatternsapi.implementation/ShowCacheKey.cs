using cloudpatternsapi.dto;
using cloudpatternsapi.interfaces;
using cloudpatternsapi.models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace cloudpatternsapi.implementation
{
    public class ShowCacheKey : ICacheKey<IList<ShowDto>>
    {
            private readonly ShowParams _showParams;
            public ShowCacheKey(ShowParams showParams)
            {
                _showParams = showParams;
            }
        public string CacheKey => $"Show_{_showParams}";
    }
}
