using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace cloudpatternsapi.interfaces.services
{
    public interface IHealthCheckService
    {
        Task<string> DatabaseHealthCheck();
        Task<string> ApiHealthCheck();
        Task<string> DiskSizeCheck();
    }
}
