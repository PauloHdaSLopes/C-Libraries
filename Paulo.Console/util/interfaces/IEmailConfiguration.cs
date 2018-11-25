using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Paulo.Console.interfaces
{
    public interface IEmailConfiguration
    {
        string  Server { get; }
        int     Port { get; }
        string  Username { get; set; }
        string  Password { get; set; }
    }
}
