using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace cloudpatternsapi.interfaces
{
    public interface IEmailService
    {
        void SendEmail(string subject, string? recipient, string? messageBody);
    }
}
