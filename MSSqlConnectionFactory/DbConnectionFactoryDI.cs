using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MSSqlConnectionFactory
{
    public static class DbConnectionFactoryDI
    {
        public static IServiceCollection AddDbFactory(this IServiceCollection collection, IDictionary<string?, string?> connections)
        {
            if (collection == null) throw new ArgumentNullException(nameof(collection));
            return collection.AddSingleton<IDbConnectionFactory, DbConnectionFactory>(factory => new DbConnectionFactory(connections));
        }
    }
}
