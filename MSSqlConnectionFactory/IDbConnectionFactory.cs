using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MSSqlConnectionFactory
{
    public interface IDbConnectionFactory
    {
        IDbConnection MSSqlConnection(string? connectionName);
    }
}
