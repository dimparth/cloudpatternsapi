using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace cloudpatternsapi.interfaces.services
{
    public interface ICacheService
    {
        void Add<TItem>(TItem item, ICacheKey<TItem> key);

        TItem? Get<TItem>(ICacheKey<TItem> key) where TItem : class;
    }
}
